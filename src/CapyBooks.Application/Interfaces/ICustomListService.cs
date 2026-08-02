using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Common;
using CapyBooks.Application.DTOs.CustomLists;

namespace CapyBooks.Application.Interfaces;

public interface ICustomListService
{
    Task<PagedResultDto<CustomListDto>> SearchAsync(CustomListSearchQueryDto query, CancellationToken cancellationToken = default);

    Task<CustomListDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CustomListDetailDto> CreateAsync(Guid userId, CreateCustomListRequestDto request, CancellationToken cancellationToken = default);

    Task<CustomListDetailDto> UpdateAsync(Guid id, Guid userId, UpdateCustomListRequestDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<CustomListDetailDto> AddItemAsync(Guid id, Guid userId, AddListItemRequestDto request, CancellationToken cancellationToken = default);

    Task<CustomListDetailDto> RemoveItemAsync(Guid id, Guid userId, Guid bookId, CancellationToken cancellationToken = default);

    Task<CustomListDetailDto> ReorderItemAsync(Guid id, Guid userId, Guid bookId, ReorderListItemRequestDto request, CancellationToken cancellationToken = default);
}
