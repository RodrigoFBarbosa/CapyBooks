using CapyBooks.Application.Common.Exceptions;
using CapyBooks.Application.DTOs.Reviews;
using CapyBooks.Application.Services;
using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Interfaces;
using Moq;

namespace CapyBooks.Application.Tests;

public class ReviewServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IBookRepository> _bookRepositoryMock = new();
    private readonly Mock<IReviewRepository> _reviewRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly ReviewService _sut;

    public ReviewServiceTests()
    {
        _unitOfWorkMock.Setup(u => u.Books).Returns(_bookRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Reviews).Returns(_reviewRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _sut = new ReviewService(_unitOfWorkMock.Object);
    }

    private User SetUpReviewer()
    {
        var reviewer = User.CreateLocal("Rodrigo", "rodrigo@example.com", "hash");
        _userRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([reviewer]);
        return reviewer;
    }

    [Fact]
    public async Task GetByBookAsync_ReturnsPagedResultWithUserNames()
    {
        var reviewer = SetUpReviewer();
        var book = new Book("Duna", "Frank Herbert", Guid.NewGuid());
        var review = new Review(book.Id, reviewer.Id, 5, "Excelente");

        _reviewRepositoryMock
            .Setup(r => r.GetByBookAsync(book.Id, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(([review], 1));

        var result = await _sut.GetByBookAsync(book.Id, new ReviewSearchQueryDto());

        Assert.Single(result.Items);
        Assert.Equal("Rodrigo", result.Items[0].UserName);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task GetByUserAndBookAsync_NotFound_ReturnsNull()
    {
        _reviewRepositoryMock
            .Setup(r => r.GetByUserAndBookAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        var result = await _sut.GetByUserAndBookAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_BookNotFound_ThrowsNotFoundException()
    {
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), new CreateReviewRequestDto(5, "Ótimo")));
    }

    [Fact]
    public async Task CreateAsync_AlreadyReviewed_ThrowsConflictException()
    {
        var book = new Book("Duna", "Frank Herbert", Guid.NewGuid());
        var userId = Guid.NewGuid();
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>())).ReturnsAsync(book);
        _reviewRepositoryMock
            .Setup(r => r.GetByUserAndBookAsync(userId, book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Review(book.Id, userId, 4));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.CreateAsync(book.Id, userId, new CreateReviewRequestDto(5, "Ótimo")));
    }

    [Fact]
    public async Task CreateAsync_Valid_CreatesReview()
    {
        var reviewer = SetUpReviewer();
        var book = new Book("Duna", "Frank Herbert", Guid.NewGuid());
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>())).ReturnsAsync(book);
        _reviewRepositoryMock
            .Setup(r => r.GetByUserAndBookAsync(reviewer.Id, book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        var result = await _sut.CreateAsync(book.Id, reviewer.Id, new CreateReviewRequestDto(5, "Ótimo"));

        Assert.Equal(5, result.Rating);
        _reviewRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsNotFoundException()
    {
        _reviewRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdateReviewRequestDto(3, null)));
    }

    [Fact]
    public async Task UpdateAsync_NotOwner_ThrowsForbiddenException()
    {
        var review = new Review(Guid.NewGuid(), Guid.NewGuid(), 4);
        _reviewRepositoryMock.Setup(r => r.GetByIdAsync(review.Id, It.IsAny<CancellationToken>())).ReturnsAsync(review);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.UpdateAsync(review.Id, Guid.NewGuid(), new UpdateReviewRequestDto(3, null)));
    }

    [Fact]
    public async Task UpdateAsync_Owner_UpdatesReview()
    {
        var reviewer = SetUpReviewer();
        var review = new Review(Guid.NewGuid(), reviewer.Id, 4);
        _reviewRepositoryMock.Setup(r => r.GetByIdAsync(review.Id, It.IsAny<CancellationToken>())).ReturnsAsync(review);

        var result = await _sut.UpdateAsync(review.Id, reviewer.Id, new UpdateReviewRequestDto(2, "Mudei de ideia"));

        Assert.Equal(2, result.Rating);
        Assert.Equal("Mudei de ideia", result.Comment);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ThrowsNotFoundException()
    {
        _reviewRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(Guid.NewGuid(), Guid.NewGuid(), isAdmin: false));
    }

    [Fact]
    public async Task DeleteAsync_NotOwnerNotAdmin_ThrowsForbiddenException()
    {
        var review = new Review(Guid.NewGuid(), Guid.NewGuid(), 4);
        _reviewRepositoryMock.Setup(r => r.GetByIdAsync(review.Id, It.IsAny<CancellationToken>())).ReturnsAsync(review);

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.DeleteAsync(review.Id, Guid.NewGuid(), isAdmin: false));
    }

    [Fact]
    public async Task DeleteAsync_Owner_RemovesReview()
    {
        var userId = Guid.NewGuid();
        var review = new Review(Guid.NewGuid(), userId, 4);
        _reviewRepositoryMock.Setup(r => r.GetByIdAsync(review.Id, It.IsAny<CancellationToken>())).ReturnsAsync(review);

        await _sut.DeleteAsync(review.Id, userId, isAdmin: false);

        _reviewRepositoryMock.Verify(r => r.Remove(review), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_AdminNotOwner_RemovesReview()
    {
        var review = new Review(Guid.NewGuid(), Guid.NewGuid(), 4);
        _reviewRepositoryMock.Setup(r => r.GetByIdAsync(review.Id, It.IsAny<CancellationToken>())).ReturnsAsync(review);

        await _sut.DeleteAsync(review.Id, Guid.NewGuid(), isAdmin: true);

        _reviewRepositoryMock.Verify(r => r.Remove(review), Times.Once);
    }
}
