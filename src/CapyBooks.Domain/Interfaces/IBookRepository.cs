using CapyBooks.Domain.Entities;

namespace CapyBooks.Domain.Interfaces;

public interface IBookRepository : IRepository<Book>
{
    Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default);
}
