using CapyBooks.Domain.Interfaces;
using CapyBooks.Infrastructure.Persistence;

namespace CapyBooks.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly CapyBooksDbContext _context;

    public UnitOfWork(
        CapyBooksDbContext context,
        IUserRepository users,
        IBookRepository books,
        IGenreRepository genres,
        IReviewRepository reviews,
        IBookshelfRepository bookshelves,
        ICustomListRepository customLists,
        IReadingLinkRepository readingLinks,
        IRefreshTokenRepository refreshTokens)
    {
        _context = context;
        Users = users;
        Books = books;
        Genres = genres;
        Reviews = reviews;
        Bookshelves = bookshelves;
        CustomLists = customLists;
        ReadingLinks = readingLinks;
        RefreshTokens = refreshTokens;
    }

    public IUserRepository Users { get; }
    public IBookRepository Books { get; }
    public IGenreRepository Genres { get; }
    public IReviewRepository Reviews { get; }
    public IBookshelfRepository Bookshelves { get; }
    public ICustomListRepository CustomLists { get; }
    public IReadingLinkRepository ReadingLinks { get; }
    public IRefreshTokenRepository RefreshTokens { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
