using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Interfaces;
using CapyBooks.Infrastructure.Persistence;

namespace CapyBooks.Infrastructure.Repositories;

public class ReadingLinkRepository : Repository<ReadingLink>, IReadingLinkRepository
{
    public ReadingLinkRepository(CapyBooksDbContext context) : base(context)
    {
    }
}
