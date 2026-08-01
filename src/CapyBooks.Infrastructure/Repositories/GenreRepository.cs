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
}
