using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Common;
using CapyBooks.Application.DTOs.Users;

namespace CapyBooks.Application.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResultDto<UserDto>> SearchAsync(UserSearchQueryDto query, CancellationToken cancellationToken = default);

    Task<UserDto> ChangeRoleAsync(Guid id, UpdateUserRoleRequestDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default);
}
