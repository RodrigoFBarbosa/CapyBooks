using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Interfaces;
using CapyBooks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CapyBooks.Infrastructure.Repositories;

public class GenreRepository : Repository<Genre>, IGenreRepository
{
    public GenreRepository(CapyBooksDbContext context) : base(context)
    {
    }

    public Task<Genre?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(g => g.Name == name, cancellationToken);

    public async Task<IReadOnlyList<Genre>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await DbSet.OrderBy(g => g.Name).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Genre>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) =>
        await DbSet.Where(g => ids.Contains(g.Id)).ToListAsync(cancellationToken);
}
