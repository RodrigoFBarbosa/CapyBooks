using CapyBooks.Application.Common.Exceptions;
using CapyBooks.Application.DTOs.ReadingLinks;
using CapyBooks.Application.Services;
using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Interfaces;
using Moq;

namespace CapyBooks.Application.Tests;

public class ReadingLinkServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IBookRepository> _bookRepositoryMock = new();
    private readonly Mock<IReadingLinkRepository> _readingLinkRepositoryMock = new();
    private readonly ReadingLinkService _sut;

    public ReadingLinkServiceTests()
    {
        _unitOfWorkMock.Setup(u => u.Books).Returns(_bookRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.ReadingLinks).Returns(_readingLinkRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _sut = new ReadingLinkService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetByBookAsync_ReturnsMappedLinks()
    {
        var bookId = Guid.NewGuid();
        var link = new ReadingLink(bookId, "Domínio Público", "https://www.dominiopublico.gov.br/livro");

        _readingLinkRepositoryMock
            .Setup(r => r.GetByBookAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([link]);

        var result = await _sut.GetByBookAsync(bookId);

        Assert.Single(result);
        Assert.Equal("Domínio Público", result[0].SourceName);
    }

    [Fact]
    public async Task CreateAsync_BookNotFound_ThrowsNotFoundException()
    {
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateAsync(Guid.NewGuid(), new CreateReadingLinkRequestDto("Gutenberg", "https://www.gutenberg.org/ebooks/1")));
    }

    [Fact]
    public async Task CreateAsync_Valid_CreatesReadingLink()
    {
        var book = new Book("Duna", "Frank Herbert", Guid.NewGuid());
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>())).ReturnsAsync(book);

        var result = await _sut.CreateAsync(book.Id, new CreateReadingLinkRequestDto("Project Gutenberg", "https://www.gutenberg.org/ebooks/1"));

        Assert.Equal(book.Id, result.BookId);
        Assert.Equal("Project Gutenberg", result.SourceName);
        _readingLinkRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ReadingLink>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsNotFoundException()
    {
        _readingLinkRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReadingLink?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateAsync(Guid.NewGuid(), new UpdateReadingLinkRequestDto("Gutenberg", "https://www.gutenberg.org/ebooks/1")));
    }

    [Fact]
    public async Task UpdateAsync_Found_UpdatesSourceAndUrl()
    {
        var link = new ReadingLink(Guid.NewGuid(), "Gutenberg", "https://www.gutenberg.org/ebooks/1");
        _readingLinkRepositoryMock.Setup(r => r.GetByIdAsync(link.Id, It.IsAny<CancellationToken>())).ReturnsAsync(link);

        var result = await _sut.UpdateAsync(link.Id, new UpdateReadingLinkRequestDto("Domínio Público", "https://www.dominiopublico.gov.br/livro"));

        Assert.Equal("Domínio Público", result.SourceName);
        Assert.Equal("https://www.dominiopublico.gov.br/livro", result.Url);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ThrowsNotFoundException()
    {
        _readingLinkRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReadingLink?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_Found_RemovesReadingLink()
    {
        var link = new ReadingLink(Guid.NewGuid(), "Gutenberg", "https://www.gutenberg.org/ebooks/1");
        _readingLinkRepositoryMock.Setup(r => r.GetByIdAsync(link.Id, It.IsAny<CancellationToken>())).ReturnsAsync(link);

        await _sut.DeleteAsync(link.Id);

        _readingLinkRepositoryMock.Verify(r => r.Remove(link), Times.Once);
    }
}
