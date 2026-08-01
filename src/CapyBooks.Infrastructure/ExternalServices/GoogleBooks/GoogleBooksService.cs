using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using CapyBooks.Application.DTOs.Books;

namespace CapyBooks.Infrastructure.ExternalServices.GoogleBooks;

public class GoogleBooksService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient;

    public GoogleBooksService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<ExternalBookResultDto>> SearchByTitleAsync(string title, CancellationToken cancellationToken = default)
    {
        var response = await GetAsync($"volumes?q={Uri.EscapeDataString("intitle:" + title)}&maxResults=10", cancellationToken);
        return MapItems(response);
    }

    public async Task<ExternalBookResultDto?> SearchByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
    {
        var response = await GetAsync($"volumes?q={Uri.EscapeDataString("isbn:" + isbn)}", cancellationToken);
        return MapItems(response).FirstOrDefault();
    }

    private async Task<GoogleBooksResponse?> GetAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<GoogleBooksResponse>(url, JsonOptions, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            return null;
        }
    }

    private static List<ExternalBookResultDto> MapItems(GoogleBooksResponse? response)
    {
        if (response?.Items is null)
            return [];

        return response.Items
            .Where(i => !string.IsNullOrWhiteSpace(i.VolumeInfo?.Title))
            .Select(i =>
            {
                var info = i.VolumeInfo!;
                var isbn = info.IndustryIdentifiers?.FirstOrDefault(x => x.Type == "ISBN_13")?.Identifier
                    ?? info.IndustryIdentifiers?.FirstOrDefault(x => x.Type == "ISBN_10")?.Identifier;

                return new ExternalBookResultDto(
                    info.Title!,
                    info.Authors?.FirstOrDefault() ?? "Desconhecido",
                    isbn,
                    info.Description,
                    info.ImageLinks?.Thumbnail,
                    ExtractYear(info.PublishedDate),
                    null,
                    i.Id,
                    info.Categories ?? []);
            })
            .ToList();
    }

    private static int? ExtractYear(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
            return null;

        var match = Regex.Match(date, @"\d{4}");
        return match.Success ? int.Parse(match.Value) : null;
    }
}
