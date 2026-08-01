using CapyBooks.Application.Common.Exceptions;
using CapyBooks.Application.DTOs.Bookshelves;
using CapyBooks.Application.Services;
using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Enums;
using CapyBooks.Domain.Interfaces;
using Moq;

namespace CapyBooks.Application.Tests;

public class BookshelfServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IBookRepository> _bookRepositoryMock = new();
    private readonly Mock<IBookshelfRepository> _bookshelfRepositoryMock = new();
    private readonly BookshelfService _sut;

    public BookshelfServiceTests()
    {
        _unitOfWorkMock.Setup(u => u.Books).Returns(_bookRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Bookshelves).Returns(_bookshelfRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _sut = new BookshelfService(_unitOfWorkMock.Object);
    }

    private Book SetUpBook()
    {
        var book = new Book("Duna", "Frank Herbert", Guid.NewGuid());
        _bookRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([book]);
        return book;
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsPagedResultWithBookInfo()
    {
        var book = SetUpBook();
        var userId = Guid.NewGuid();
        var entry = new Bookshelf(userId, book.Id, BookshelfStatus.Reading);

        _bookshelfRepositoryMock
            .Setup(r => r.GetByUserAsync(userId, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(([entry], 1));

        var result = await _sut.GetByUserAsync(userId, new BookshelfSearchQueryDto());

        Assert.Single(result.Items);
        Assert.Equal("Duna", result.Items[0].BookTitle);
        Assert.Equal("Reading", result.Items[0].Status);
    }

    [Fact]
    public async Task GetByUserAndBookAsync_NotFound_ReturnsNull()
    {
        _bookshelfRepositoryMock
            .Setup(r => r.GetByUserAndBookAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bookshelf?)null);

        var result = await _sut.GetByUserAndBookAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task SetStatusAsync_BookNotFound_ThrowsNotFoundException()
    {
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.SetStatusAsync(Guid.NewGuid(), Guid.NewGuid(), new SetBookshelfStatusRequestDto("Reading")));
    }

    [Fact]
    public async Task SetStatusAsync_NoExistingEntry_CreatesNewEntry()
    {
        var book = SetUpBook();
        var userId = Guid.NewGuid();
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>())).ReturnsAsync(book);
        _bookshelfRepositoryMock
            .Setup(r => r.GetByUserAndBookAsync(userId, book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bookshelf?)null);

        var result = await _sut.SetStatusAsync(userId, book.Id, new SetBookshelfStatusRequestDto("WantToRead"));

        Assert.Equal("WantToRead", result.Status);
        _bookshelfRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Bookshelf>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetStatusAsync_ExistingEntry_UpdatesStatus()
    {
        var book = SetUpBook();
        var userId = Guid.NewGuid();
        var entry = new Bookshelf(userId, book.Id, BookshelfStatus.WantToRead);

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>())).ReturnsAsync(book);
        _bookshelfRepositoryMock
            .Setup(r => r.GetByUserAndBookAsync(userId, book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entry);

        var result = await _sut.SetStatusAsync(userId, book.Id, new SetBookshelfStatusRequestDto("Read"));

        Assert.Equal("Read", result.Status);
        Assert.Equal(BookshelfStatus.Read, entry.Status);
        _bookshelfRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Bookshelf>(), It.IsAny<CancellationToken>()), Times.Never);
        _bookshelfRepositoryMock.Verify(r => r.Update(entry), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_NotFound_ThrowsNotFoundException()
    {
        _bookshelfRepositoryMock
            .Setup(r => r.GetByUserAndBookAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bookshelf?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.RemoveAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task RemoveAsync_Found_RemovesEntry()
    {
        var entry = new Bookshelf(Guid.NewGuid(), Guid.NewGuid(), BookshelfStatus.Read);
        _bookshelfRepositoryMock
            .Setup(r => r.GetByUserAndBookAsync(entry.UserId, entry.BookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entry);

        await _sut.RemoveAsync(entry.UserId, entry.BookId);

        _bookshelfRepositoryMock.Verify(r => r.Remove(entry), Times.Once);
    }
}
