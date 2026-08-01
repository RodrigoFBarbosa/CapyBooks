using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Interfaces;
using CapyBooks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CapyBooks.Infrastructure.Repositories;

public class CustomListRepository : Repository<CustomList>, ICustomListRepository
{
    public CustomListRepository(CapyBooksDbContext context) : base(context)
    {
    }

    public override Task<CustomList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        DbSet.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
}
