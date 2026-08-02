namespace CapyBooks.Application.DTOs;

public record CustomListDetailDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string Name,
    string? Description,
    IReadOnlyList<CustomListItemDto> Items);
