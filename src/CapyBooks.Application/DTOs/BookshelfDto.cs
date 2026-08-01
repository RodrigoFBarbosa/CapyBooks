namespace CapyBooks.Application.DTOs;

public record BookshelfDto(
    Guid Id,
    Guid BookId,
    string BookTitle,
    string BookAuthor,
    string? BookCoverUrl,
    string Status);
