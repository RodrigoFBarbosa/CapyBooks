using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using CapyBooks.Application.DTOs.Books;

namespace CapyBooks.Infrastructure.ExternalServices.OpenLibrary;

public class OpenLibraryService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient;

    public OpenLibraryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ExternalBookResultDto>> SearchByTitleAsync(string title, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"search.json?q={Uri.EscapeDataString(title)}&fields=title,author_name,isbn,cover_i,first_publish_year,subject,key&limit=10";
            var response = await _httpClient.GetFromJsonAsync<OpenLibrarySearchResponse>(url, JsonOptions, cancellationToken);

            if (response?.Docs is null)
                return [];

            return response.Docs
                .Where(d => !string.IsNullOrWhiteSpace(d.Title))
                .Select(ToExternalBookResult)
                .ToList();
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            return [];
        }
    }

    public async Task<ExternalBookResultDto?> SearchByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"api/books?bibkeys=ISBN:{Uri.EscapeDataString(isbn)}&format=json&jscmd=data";
            var response = await _httpClient.GetFromJsonAsync<Dictionary<string, OpenLibraryBookData>>(url, JsonOptions, cancellationToken);

            if (response is null || !response.TryGetValue($"ISBN:{isbn}", out var book) || string.IsNullOrWhiteSpace(book.Title))
                return null;

            return new ExternalBookResultDto(
                book.Title,
                book.Authors?.FirstOrDefault()?.Name ?? "Desconhecido",
                isbn,
                null,
                book.Cover?.Large ?? book.Cover?.Medium,
                ExtractYear(book.PublishDate),
                null,
                null,
                (book.Subjects ?? [])
                    .Select(s => s.Name)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Take(10)
                    .Select(n => n!)
                    .ToList());
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            return null;
        }
    }

    private static ExternalBookResultDto ToExternalBookResult(OpenLibraryDoc doc) => new(
        doc.Title!,
        doc.AuthorName?.FirstOrDefault() ?? "Desconhecido",
        doc.Isbn?.FirstOrDefault(),
        null,
        doc.CoverId.HasValue ? $"https://covers.openlibrary.org/b/id/{doc.CoverId}-L.jpg" : null,
        doc.FirstPublishYear,
        doc.Key,
        null,
        (doc.Subject ?? []).Take(10).ToList());

    private static int? ExtractYear(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
            return null;

        var match = Regex.Match(date, @"\d{4}");
        return match.Success ? int.Parse(match.Value) : null;
    }
}
