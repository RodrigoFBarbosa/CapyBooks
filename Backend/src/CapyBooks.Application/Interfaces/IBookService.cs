using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Books;
using CapyBooks.Application.DTOs.Common;

namespace CapyBooks.Application.Interfaces;

public interface IBookService
{
    Task<BookDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResultDto<BookDto>> SearchAsync(BookSearchQueryDto query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExternalBookResultDto>> SearchExternalAsync(ExternalBookSearchQueryDto query, CancellationToken cancellationToken = default);

    Task<BookDto> CreateAsync(CreateBookRequestDto request, Guid adminId, CancellationToken cancellationToken = default);

    Task<BookDto> UpdateAsync(Guid id, UpdateBookRequestDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
