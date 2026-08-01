namespace CapyBooks.Application.DTOs;

public record BookDto(
    Guid Id,
    string Title,
    string Author,
    string? Isbn,
    string? Synopsis,
    string? CoverUrl,
    int? PublishedYear,
    string? OpenLibraryId,
    string? GoogleBooksId,
    IReadOnlyList<GenreDto> Genres,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
