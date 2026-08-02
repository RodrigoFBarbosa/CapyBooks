namespace CapyBooks.Application.DTOs;

public record CustomListDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string Name,
    string? Description,
    int ItemCount);
