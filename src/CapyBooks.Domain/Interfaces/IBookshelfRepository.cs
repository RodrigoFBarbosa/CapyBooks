using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Enums;

namespace CapyBooks.Domain.Interfaces;

public interface IBookshelfRepository : IRepository<Bookshelf>
{
    Task<Bookshelf?> GetByUserAndBookAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Bookshelf> Items, int TotalCount)> GetByUserAsync(
        Guid userId,
        BookshelfStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
