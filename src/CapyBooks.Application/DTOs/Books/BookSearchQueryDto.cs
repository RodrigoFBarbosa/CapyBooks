namespace CapyBooks.Application.DTOs.Books;

public record BookSearchQueryDto(int Page = 1, int PageSize = 20, string? Search = null, Guid? GenreId = null);
