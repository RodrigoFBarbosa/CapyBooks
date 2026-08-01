using CapyBooks.Domain.Entities;
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
}
