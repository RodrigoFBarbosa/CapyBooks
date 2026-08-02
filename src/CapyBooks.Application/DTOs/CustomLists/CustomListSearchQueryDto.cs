namespace CapyBooks.Application.DTOs.CustomLists;

public record CustomListSearchQueryDto(int Page = 1, int PageSize = 20, Guid? UserId = null);
