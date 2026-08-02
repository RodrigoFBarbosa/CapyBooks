using CapyBooks.Domain.Entities;

namespace CapyBooks.Domain.Interfaces;

public interface IGenreRepository : IRepository<Genre>
{
    Task<Genre?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Genre>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Genre>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
