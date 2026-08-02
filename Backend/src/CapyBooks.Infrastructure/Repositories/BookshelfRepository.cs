using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Enums;
using CapyBooks.Domain.Interfaces;
using CapyBooks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CapyBooks.Infrastructure.Repositories;

public class BookshelfRepository : Repository<Bookshelf>, IBookshelfRepository
{
    public BookshelfRepository(CapyBooksDbContext context) : base(context)
    {
    }

    public Task<Bookshelf?> GetByUserAndBookAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(b => b.UserId == userId && b.BookId == bookId, cancellationToken);

    public async Task<(IReadOnlyList<Bookshelf> Items, int TotalCount)> GetByUserAsync(
        Guid userId,
        BookshelfStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(b => b.UserId == userId);

        if (status.HasValue)
            query = query.Where(b => b.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(b => b.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
