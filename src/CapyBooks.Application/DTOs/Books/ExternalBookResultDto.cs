namespace CapyBooks.Application.DTOs.Books;

public record ExternalBookResultDto(
    string Title,
    string Author,
    string? Isbn,
    string? Synopsis,
    string? CoverUrl,
    int? PublishedYear,
    string? OpenLibraryId,
    string? GoogleBooksId,
    IReadOnlyList<string> Subjects);
