namespace CapyBooks.Application.DTOs.Books;

public record UpdateBookRequestDto(
    string Title,
    string Author,
    string? Isbn,
    string? Synopsis,
    string? CoverUrl,
    int? PublishedYear,
    IReadOnlyList<Guid> GenreIds);
