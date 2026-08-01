using CapyBooks.Domain.Common;
using CapyBooks.Domain.Exceptions;

namespace CapyBooks.Domain.Entities;

public class Review : BaseEntity
{
    private const int MinRating = 1;
    private const int MaxRating = 5;

    public Guid BookId { get; private set; }
    public Guid UserId { get; private set; }
    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Review()
    {
    }

    public Review(Guid bookId, Guid userId, int rating, string? comment = null)
    {
        ValidateRating(rating);

        BookId = bookId;
        UserId = userId;
        Rating = rating;
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(int rating, string? comment)
    {
        ValidateRating(rating);

        Rating = rating;
        Comment = comment;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateRating(int rating)
    {
        if (rating < MinRating || rating > MaxRating)
            throw new DomainException($"A nota da avaliação deve estar entre {MinRating} e {MaxRating}.");
    }
}
