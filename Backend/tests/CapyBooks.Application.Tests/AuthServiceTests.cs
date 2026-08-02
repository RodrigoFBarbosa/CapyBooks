using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using CapyBooks.Application.Common.Exceptions;
using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Auth;
using CapyBooks.Application.Interfaces;
using CapyBooks.Application.Services;
using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Interfaces;
using Moq;

namespace CapyBooks.Application.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _tokenGeneratorMock = new();
    private readonly Mock<IGoogleTokenValidator> _googleTokenValidatorMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.RefreshTokens).Returns(_refreshTokenRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _tokenGeneratorMock.Setup(t => t.GenerateAccessToken(It.IsAny<User>()))
            .Returns(("access-token", DateTime.UtcNow.AddMinutes(15)));
        _tokenGeneratorMock.Setup(t => t.GenerateRefreshToken())
            .Returns(("raw-refresh-token", DateTime.UtcNow.AddDays(7)));

        _mapperMock.Setup(m => m.Map<UserDto>(It.IsAny<object>()))
            .Returns<object>(src =>
            {
                var user = (User)src;
                return new UserDto(user.Id, user.Name, user.Email, user.Role.ToString(), user.CreatedAt);
            });

        _sut = new AuthService(
            _unitOfWorkMock.Object,
            _passwordHasherMock.Object,
            _tokenGeneratorMock.Object,
            _googleTokenValidatorMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithNewEmail_CreatesUserAndReturnsTokens()
    {
        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync("new@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasherMock.Setup(p => p.Hash("Password123")).Returns("hashed-password");

        var result = await _sut.RegisterAsync(new RegisterRequestDto("Rodrigo", "new@example.com", "Password123"));

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("raw-refresh-token", result.RefreshToken);
        Assert.Equal("new@example.com", result.User.Email);
        _userRepositoryMock.Verify(
            r => r.AddAsync(It.Is<User>(u => u.Email == "new@example.com" && u.PasswordHash == "hashed-password"), It.IsAny<CancellationToken>()),
            Times.Once);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsConflictException()
    {
        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync("taken@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.RegisterAsync(new RegisterRequestDto("Rodrigo", "taken@example.com", "Password123")));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokens()
    {
        var user = User.CreateLocal("Rodrigo", "rodrigo@example.com", "hashed-password");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("rodrigo@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.Verify("Password123", "hashed-password")).Returns(true);

        var result = await _sut.LoginAsync(new LoginRequestDto("rodrigo@example.com", "Password123"));

        Assert.Equal("access-token", result.AccessToken);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsAuthenticationException()
    {
        var user = User.CreateLocal("Rodrigo", "rodrigo@example.com", "hashed-password");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("rodrigo@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.Verify("WrongPassword", "hashed-password")).Returns(false);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            _sut.LoginAsync(new LoginRequestDto("rodrigo@example.com", "WrongPassword")));
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ThrowsAuthenticationException()
    {
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("unknown@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            _sut.LoginAsync(new LoginRequestDto("unknown@example.com", "Password123")));
    }

    [Fact]
    public async Task LoginAsync_WithGoogleOnlyAccount_ThrowsAuthenticationException()
    {
        var user = User.CreateFromGoogle("Rodrigo", "rodrigo@example.com", "google-123");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("rodrigo@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            _sut.LoginAsync(new LoginRequestDto("rodrigo@example.com", "AnyPassword")));
    }

    [Fact]
    public async Task LoginWithGoogleAsync_NewUser_CreatesUserAndReturnsTokens()
    {
        _googleTokenValidatorMock.Setup(g => g.ValidateAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfo("google-123", "new@example.com", "Rodrigo"));
        _userRepositoryMock.Setup(r => r.GetByGoogleIdAsync("google-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("new@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.LoginWithGoogleAsync(new GoogleLoginRequestDto("valid-token"));

        Assert.Equal("new@example.com", result.User.Email);
        _userRepositoryMock.Verify(
            r => r.AddAsync(It.Is<User>(u => u.GoogleId == "google-123"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginWithGoogleAsync_ExistingEmailNotLinked_LinksGoogleAccount()
    {
        var existingUser = User.CreateLocal("Rodrigo", "rodrigo@example.com", "hashed-password");
        _googleTokenValidatorMock.Setup(g => g.ValidateAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GoogleUserInfo("google-123", "rodrigo@example.com", "Rodrigo"));
        _userRepositoryMock.Setup(r => r.GetByGoogleIdAsync("google-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync("rodrigo@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        await _sut.LoginWithGoogleAsync(new GoogleLoginRequestDto("valid-token"));

        Assert.Equal("google-123", existingUser.GoogleId);
        _userRepositoryMock.Verify(r => r.Update(existingUser), Times.Once);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginWithGoogleAsync_InvalidToken_ThrowsAuthenticationException()
    {
        _googleTokenValidatorMock.Setup(g => g.ValidateAsync("invalid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((GoogleUserInfo?)null);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            _sut.LoginWithGoogleAsync(new GoogleLoginRequestDto("invalid-token")));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_RotatesAndReturnsNewTokens()
    {
        var user = User.CreateLocal("Rodrigo", "rodrigo@example.com", "hashed-password");
        var storedToken = new RefreshToken(user.Id, ComputeHash("old-raw-token"), DateTime.UtcNow.AddDays(1));

        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync(ComputeHash("old-raw-token"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.RefreshTokenAsync(new RefreshTokenRequestDto("old-raw-token"));

        Assert.False(storedToken.IsActive);
        Assert.Equal("raw-refresh-token", result.RefreshToken);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ThrowsAuthenticationException()
    {
        var expiredToken = new RefreshToken(Guid.NewGuid(), ComputeHash("expired-token"), DateTime.UtcNow.AddDays(-1));
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync(ComputeHash("expired-token"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredToken);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            _sut.RefreshTokenAsync(new RefreshTokenRequestDto("expired-token")));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithUnknownToken_ThrowsAuthenticationException()
    {
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            _sut.RefreshTokenAsync(new RefreshTokenRequestDto("unknown-token")));
    }

    private static string ComputeHash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
