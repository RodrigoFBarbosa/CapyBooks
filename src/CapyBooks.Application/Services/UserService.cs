using AutoMapper;
using CapyBooks.Application.Common.Exceptions;
using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Common;
using CapyBooks.Application.DTOs.Users;
using CapyBooks.Application.Interfaces;
using CapyBooks.Domain.Enums;
using CapyBooks.Domain.Interfaces;

namespace CapyBooks.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        return user is null ? null : _mapper.Map<UserDto>(user);
    }

    public async Task<PagedResultDto<UserDto>> SearchAsync(UserSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Users.SearchAsync(query.Search, query.Page, query.PageSize, cancellationToken);
        var dtos = items.Select(u => _mapper.Map<UserDto>(u)).ToList();

        return new PagedResultDto<UserDto>(dtos, query.Page, query.PageSize, totalCount);
    }

    public async Task<UserDto> ChangeRoleAsync(Guid id, UpdateUserRoleRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado.");

        var role = Enum.Parse<UserRole>(request.Role, ignoreCase: true);
        user.ChangeRole(role);

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }

    public async Task DeleteAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        if (id == currentUserId)
            throw new ConflictException("Você não pode remover sua própria conta.");

        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Usuário não encontrado.");

        _unitOfWork.Users.Remove(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
