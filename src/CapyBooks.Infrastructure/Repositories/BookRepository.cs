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

    public async Task<(IReadOnlyList<Book> Items, int TotalCount)> SearchAsync(
        string? search,
        Guid? genreId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Include(b => b.Genres).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(b =>
                EF.Functions.ILike(b.Title, $"%{search}%") ||
                EF.Functions.ILike(b.Author, $"%{search}%"));
        }

        if (genreId.HasValue)
            query = query.Where(b => b.Genres.Any(g => g.Id == genreId.Value));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(b => b.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
