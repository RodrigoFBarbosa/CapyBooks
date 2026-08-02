using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using CapyBooks.Application.Common.Exceptions;
using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Auth;
using CapyBooks.Application.Interfaces;
using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Interfaces;

namespace CapyBooks.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IMapper _mapper;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        IGoogleTokenValidator googleTokenValidator,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _googleTokenValidator = googleTokenValidator;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Users.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new ConflictException("Este e-mail já está cadastrado.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.CreateLocal(request.Name, request.Email, passwordHash);

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || user.PasswordHash is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new AuthenticationException("E-mail ou senha inválidos.");

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> LoginWithGoogleAsync(GoogleLoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var googleUser = await _googleTokenValidator.ValidateAsync(request.IdToken, cancellationToken)
            ?? throw new AuthenticationException("Token do Google inválido.");

        var user = await _unitOfWork.Users.GetByGoogleIdAsync(googleUser.GoogleId, cancellationToken);

        if (user is null)
        {
            user = await _unitOfWork.Users.GetByEmailAsync(googleUser.Email, cancellationToken);

            if (user is null)
            {
                user = User.CreateFromGoogle(googleUser.Name, googleUser.Email, googleUser.GoogleId);
                await _unitOfWork.Users.AddAsync(user, cancellationToken);
            }
            else
            {
                user.LinkGoogleAccount(googleUser.GoogleId);
                _unitOfWork.Users.Update(user);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var storedToken = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
            throw new AuthenticationException("Refresh token inválido ou expirado.");

        var user = await _unitOfWork.Users.GetByIdAsync(storedToken.UserId, cancellationToken)
            ?? throw new AuthenticationException("Usuário não encontrado.");

        var (newRawRefreshToken, newRefreshTokenExpiresAt) = _tokenGenerator.GenerateRefreshToken();
        var newTokenHash = HashToken(newRawRefreshToken);

        storedToken.Revoke(newTokenHash);
        _unitOfWork.RefreshTokens.Update(storedToken);

        var newRefreshToken = new RefreshToken(user.Id, newTokenHash, newRefreshTokenExpiresAt);
        await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var (accessToken, accessTokenExpiresAt) = _tokenGenerator.GenerateAccessToken(user);

        return new AuthResponseDto(
            accessToken,
            accessTokenExpiresAt,
            newRawRefreshToken,
            newRefreshTokenExpiresAt,
            _mapper.Map<UserDto>(user));
    }

    public async Task LogoutAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var storedToken = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (storedToken is null || !storedToken.IsActive)
            return;

        storedToken.Revoke();
        _unitOfWork.RefreshTokens.Update(storedToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        return user is null ? null : _mapper.Map<UserDto>(user);
    }

    private async Task<AuthResponseDto> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var (accessToken, accessTokenExpiresAt) = _tokenGenerator.GenerateAccessToken(user);
        var (rawRefreshToken, refreshTokenExpiresAt) = _tokenGenerator.GenerateRefreshToken();

        var refreshToken = new RefreshToken(user.Id, HashToken(rawRefreshToken), refreshTokenExpiresAt);
        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            accessToken,
            accessTokenExpiresAt,
            rawRefreshToken,
            refreshTokenExpiresAt,
            _mapper.Map<UserDto>(user));
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
