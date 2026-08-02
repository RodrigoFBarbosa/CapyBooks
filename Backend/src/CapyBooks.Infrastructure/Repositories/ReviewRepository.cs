using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Interfaces;
using CapyBooks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CapyBooks.Infrastructure.Repositories;

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(CapyBooksDbContext context) : base(context)
    {
    }

    public Task<Review?> GetByUserAndBookAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(r => r.UserId == userId && r.BookId == bookId, cancellationToken);

    public async Task<(IReadOnlyList<Review> Items, int TotalCount)> GetByBookAsync(
        Guid bookId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(r => r.BookId == bookId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
