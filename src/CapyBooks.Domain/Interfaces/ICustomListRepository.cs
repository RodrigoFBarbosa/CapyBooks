using CapyBooks.Domain.Entities;

namespace CapyBooks.Domain.Interfaces;

public interface ICustomListRepository : IRepository<CustomList>
{
    Task<(IReadOnlyList<CustomList> Items, int TotalCount)> SearchAsync(
        Guid? userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
