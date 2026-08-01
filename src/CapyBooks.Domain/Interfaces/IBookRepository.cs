using CapyBooks.Domain.Entities;

namespace CapyBooks.Domain.Interfaces;

public interface IBookRepository : IRepository<Book>
{
    Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Book> Items, int TotalCount)> SearchAsync(
        string? search,
        Guid? genreId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Book>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
