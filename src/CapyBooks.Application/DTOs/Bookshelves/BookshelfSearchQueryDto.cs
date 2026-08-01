namespace CapyBooks.Application.DTOs.Bookshelves;

public record BookshelfSearchQueryDto(int Page = 1, int PageSize = 20, string? Status = null);
