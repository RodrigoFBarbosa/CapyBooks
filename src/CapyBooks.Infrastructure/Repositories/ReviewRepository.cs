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
}
