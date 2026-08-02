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

    public async Task<(IReadOnlyList<CustomList> Items, int TotalCount)> SearchAsync(
        Guid? userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Include(c => c.Items).AsQueryable();

        if (userId.HasValue)
            query = query.Where(c => c.UserId == userId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
