using CapyBooks.Application.DTOs.Books;
using CapyBooks.Application.Interfaces;
using CapyBooks.Infrastructure.ExternalServices.GoogleBooks;
using CapyBooks.Infrastructure.ExternalServices.OpenLibrary;

namespace CapyBooks.Infrastructure.ExternalServices;

public class ExternalBookSearchService : IExternalBookSearchService
{
    private readonly OpenLibraryService _openLibraryService;
    private readonly GoogleBooksService _googleBooksService;

    public ExternalBookSearchService(OpenLibraryService openLibraryService, GoogleBooksService googleBooksService)
    {
        _openLibraryService = openLibraryService;
        _googleBooksService = googleBooksService;
    }

    public async Task<IReadOnlyList<ExternalBookResultDto>> SearchByTitleAsync(string title, CancellationToken cancellationToken = default)
    {
        var results = await _openLibraryService.SearchByTitleAsync(title, cancellationToken);

        return results.Count > 0
            ? results
            : await _googleBooksService.SearchByTitleAsync(title, cancellationToken);
    }

    public async Task<ExternalBookResultDto?> SearchByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
    {
        var openLibraryResult = await _openLibraryService.SearchByIsbnAsync(isbn, cancellationToken);

        if (openLibraryResult is not null && !string.IsNullOrWhiteSpace(openLibraryResult.Synopsis))
            return openLibraryResult;

        var googleBooksResult = await _googleBooksService.SearchByIsbnAsync(isbn, cancellationToken);

        if (openLibraryResult is null)
            return googleBooksResult;

        if (googleBooksResult is not null && string.IsNullOrWhiteSpace(openLibraryResult.Synopsis))
        {
            return openLibraryResult with
            {
                Synopsis = googleBooksResult.Synopsis,
                GoogleBooksId = googleBooksResult.GoogleBooksId
            };
        }

        return openLibraryResult;
    }
}
