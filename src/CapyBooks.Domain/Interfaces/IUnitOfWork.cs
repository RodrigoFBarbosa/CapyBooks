namespace CapyBooks.Domain.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IBookRepository Books { get; }
    IGenreRepository Genres { get; }
    IReviewRepository Reviews { get; }
    IBookshelfRepository Bookshelves { get; }
    ICustomListRepository CustomLists { get; }
    IReadingLinkRepository ReadingLinks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
