namespace CapyBooks.Application.DTOs;

public record ReviewDto(
    Guid Id,
    Guid BookId,
    Guid UserId,
    string UserName,
    int Rating,
    string? Comment,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
