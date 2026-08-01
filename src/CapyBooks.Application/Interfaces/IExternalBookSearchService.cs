using CapyBooks.Application.DTOs.Books;

namespace CapyBooks.Application.Interfaces;

public interface IExternalBookSearchService
{
    Task<IReadOnlyList<ExternalBookResultDto>> SearchByTitleAsync(string title, CancellationToken cancellationToken = default);

    Task<ExternalBookResultDto?> SearchByIsbnAsync(string isbn, CancellationToken cancellationToken = default);
}
