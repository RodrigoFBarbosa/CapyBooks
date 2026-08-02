using CapyBooks.Application.Common.Exceptions;
using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Common;
using CapyBooks.Application.DTOs.Reviews;
using CapyBooks.Application.Interfaces;
using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Interfaces;

namespace CapyBooks.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReviewService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResultDto<ReviewDto>> GetByBookAsync(Guid bookId, ReviewSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Reviews.GetByBookAsync(bookId, query.Page, query.PageSize, cancellationToken);
        var dtos = await MapWithUserNamesAsync(items, cancellationToken);

        return new PagedResultDto<ReviewDto>(dtos, query.Page, query.PageSize, totalCount);
    }

    public async Task<ReviewDto?> GetByUserAndBookAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default)
    {
        var review = await _unitOfWork.Reviews.GetByUserAndBookAsync(userId, bookId, cancellationToken);
        if (review is null)
            return null;

        var dtos = await MapWithUserNamesAsync([review], cancellationToken);
        return dtos[0];
    }

    public async Task<ReviewDto> CreateAsync(Guid bookId, Guid userId, CreateReviewRequestDto request, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Books.GetByIdAsync(bookId, cancellationToken) is null)
            throw new NotFoundException("Livro não encontrado.");

        if (await _unitOfWork.Reviews.GetByUserAndBookAsync(userId, bookId, cancellationToken) is not null)
            throw new ConflictException("Você já avaliou este livro.");

        var review = new Review(bookId, userId, request.Rating, request.Comment);

        await _unitOfWork.Reviews.AddAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dtos = await MapWithUserNamesAsync([review], cancellationToken);
        return dtos[0];
    }

    public async Task<ReviewDto> UpdateAsync(Guid id, Guid userId, UpdateReviewRequestDto request, CancellationToken cancellationToken = default)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Avaliação não encontrada.");

        if (review.UserId != userId)
            throw new ForbiddenException("Você só pode editar sua própria avaliação.");

        review.Update(request.Rating, request.Comment);

        _unitOfWork.Reviews.Update(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dtos = await MapWithUserNamesAsync([review], cancellationToken);
        return dtos[0];
    }

    public async Task DeleteAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Avaliação não encontrada.");

        if (!isAdmin && review.UserId != userId)
            throw new ForbiddenException("Você não tem permissão para remover esta avaliação.");

        _unitOfWork.Reviews.Remove(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<ReviewDto>> MapWithUserNamesAsync(IReadOnlyList<Review> reviews, CancellationToken cancellationToken)
    {
        var userIds = reviews.Select(r => r.UserId).Distinct().ToList();
        var users = await _unitOfWork.Users.GetByIdsAsync(userIds, cancellationToken);
        var userNames = users.ToDictionary(u => u.Id, u => u.Name);

        return reviews
            .Select(r => new ReviewDto(
                r.Id,
                r.BookId,
                r.UserId,
                userNames.TryGetValue(r.UserId, out var name) ? name : "Usuário removido",
                r.Rating,
                r.Comment,
                r.CreatedAt,
                r.UpdatedAt))
            .ToList();
    }
}
