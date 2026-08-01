using CapyBooks.Domain.Entities;

namespace CapyBooks.Domain.Interfaces;

public interface IBookshelfRepository : IRepository<Bookshelf>
{
    Task<Bookshelf?> GetByUserAndBookAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default);
}
