using Microsoft.EntityFrameworkCore;

namespace CapyBooks.Infrastructure.Persistence;

public class CapyBooksDbContext : DbContext
{
    public CapyBooksDbContext(DbContextOptions<CapyBooksDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CapyBooksDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
