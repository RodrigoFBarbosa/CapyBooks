using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Exceptions;

namespace CapyBooks.Domain.Tests;

public class ReviewTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Constructor_WithValidRating_CreatesReview(int rating)
    {
        var review = new Review(Guid.NewGuid(), Guid.NewGuid(), rating);

        Assert.Equal(rating, review.Rating);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Constructor_WithRatingOutOfRange_ThrowsDomainException(int rating)
    {
        Assert.Throws<DomainException>(() => new Review(Guid.NewGuid(), Guid.NewGuid(), rating));
    }

    [Fact]
    public void Update_WithInvalidRating_ThrowsDomainException()
    {
        var review = new Review(Guid.NewGuid(), Guid.NewGuid(), 3);

        Assert.Throws<DomainException>(() => review.Update(10, "comentário"));
    }

    [Fact]
    public void Update_WithValidRating_UpdatesRatingCommentAndTimestamp()
    {
        var review = new Review(Guid.NewGuid(), Guid.NewGuid(), 3);

        review.Update(5, "excelente");

        Assert.Equal(5, review.Rating);
        Assert.Equal("excelente", review.Comment);
        Assert.NotNull(review.UpdatedAt);
    }
}
