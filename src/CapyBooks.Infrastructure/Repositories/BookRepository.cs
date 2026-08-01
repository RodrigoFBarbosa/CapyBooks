using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Interfaces;
using CapyBooks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CapyBooks.Infrastructure.Repositories;

public class BookRepository : Repository<Book>, IBookRepository
{
    public BookRepository(CapyBooksDbContext context) : base(context)
    {
    }

    public override Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        DbSet.Include(b => b.Genres).FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default) =>
        DbSet.Include(b => b.Genres).FirstOrDefaultAsync(b => b.Isbn == isbn, cancellationToken);
}
