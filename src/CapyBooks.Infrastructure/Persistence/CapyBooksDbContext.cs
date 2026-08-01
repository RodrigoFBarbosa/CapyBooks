using CapyBooks.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CapyBooks.Infrastructure.Persistence;

public class CapyBooksDbContext : DbContext
{
    public CapyBooksDbContext(DbContextOptions<CapyBooksDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Bookshelf> Bookshelves => Set<Bookshelf>();
    public DbSet<CustomList> CustomLists => Set<CustomList>();
    public DbSet<ListItem> ListItems => Set<ListItem>();
    public DbSet<ReadingLink> ReadingLinks => Set<ReadingLink>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CapyBooksDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
