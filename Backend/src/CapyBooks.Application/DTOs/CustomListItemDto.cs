namespace CapyBooks.Application.DTOs;

public record CustomListItemDto(
    Guid BookId,
    string BookTitle,
    string BookAuthor,
    string? BookCoverUrl,
    int Order);
