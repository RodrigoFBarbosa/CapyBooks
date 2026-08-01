using CapyBooks.Domain.Entities;

namespace CapyBooks.Domain.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<Review?> GetByUserAndBookAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default);
}
