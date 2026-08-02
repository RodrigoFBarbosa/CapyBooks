using CapyBooks.Domain.Entities;

namespace CapyBooks.Domain.Interfaces;

public interface IReadingLinkRepository : IRepository<ReadingLink>
{
    Task<IReadOnlyList<ReadingLink>> GetByBookAsync(Guid bookId, CancellationToken cancellationToken = default);
}
