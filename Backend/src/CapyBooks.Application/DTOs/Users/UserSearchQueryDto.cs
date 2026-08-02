namespace CapyBooks.Application.DTOs.Users;

public record UserSearchQueryDto(int Page = 1, int PageSize = 20, string? Search = null);
