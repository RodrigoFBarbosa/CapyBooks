using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Common;
using CapyBooks.Application.DTOs.Reviews;

namespace CapyBooks.Application.Interfaces;

public interface IReviewService
{
    Task<PagedResultDto<ReviewDto>> GetByBookAsync(Guid bookId, ReviewSearchQueryDto query, CancellationToken cancellationToken = default);

    Task<ReviewDto?> GetByUserAndBookAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default);

    Task<ReviewDto> CreateAsync(Guid bookId, Guid userId, CreateReviewRequestDto request, CancellationToken cancellationToken = default);

    Task<ReviewDto> UpdateAsync(Guid id, Guid userId, UpdateReviewRequestDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken = default);
}
