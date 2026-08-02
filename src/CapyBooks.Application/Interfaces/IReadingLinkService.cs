using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.ReadingLinks;

namespace CapyBooks.Application.Interfaces;

public interface IReadingLinkService
{
    Task<IReadOnlyList<ReadingLinkDto>> GetByBookAsync(Guid bookId, CancellationToken cancellationToken = default);

    Task<ReadingLinkDto> CreateAsync(Guid bookId, CreateReadingLinkRequestDto request, CancellationToken cancellationToken = default);

    Task<ReadingLinkDto> UpdateAsync(Guid id, UpdateReadingLinkRequestDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
