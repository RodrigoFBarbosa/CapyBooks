using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Bookshelves;
using CapyBooks.Application.DTOs.Common;

namespace CapyBooks.Application.Interfaces;

public interface IBookshelfService
{
    Task<PagedResultDto<BookshelfDto>> GetByUserAsync(Guid userId, BookshelfSearchQueryDto query, CancellationToken cancellationToken = default);

    Task<BookshelfDto?> GetByUserAndBookAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default);

    Task<BookshelfDto> SetStatusAsync(Guid userId, Guid bookId, SetBookshelfStatusRequestDto request, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default);
}
