using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Interfaces;
using CapyBooks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CapyBooks.Infrastructure.Repositories;

public class ReadingLinkRepository : Repository<ReadingLink>, IReadingLinkRepository
{
    public ReadingLinkRepository(CapyBooksDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ReadingLink>> GetByBookAsync(Guid bookId, CancellationToken cancellationToken = default) =>
        await DbSet.Where(r => r.BookId == bookId).ToListAsync(cancellationToken);
}
