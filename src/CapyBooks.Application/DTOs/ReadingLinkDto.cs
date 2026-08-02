namespace CapyBooks.Application.DTOs;

public record ReadingLinkDto(Guid Id, Guid BookId, string SourceName, string Url);
