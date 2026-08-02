using AutoMapper;
using CapyBooks.Application.Common.Exceptions;
using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Books;
using CapyBooks.Application.Interfaces;
using CapyBooks.Application.Services;
using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Interfaces;
using Moq;

namespace CapyBooks.Application.Tests;

public class BookServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IBookRepository> _bookRepositoryMock = new();
    private readonly Mock<IGenreRepository> _genreRepositoryMock = new();
    private readonly Mock<IExternalBookSearchService> _externalBookSearchServiceMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly BookService _sut;

    public BookServiceTests()
    {
        _unitOfWorkMock.Setup(u => u.Books).Returns(_bookRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Genres).Returns(_genreRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mapperMock.Setup(m => m.Map<BookDto>(It.IsAny<object>()))
            .Returns<object>(src =>
            {
                var book = (Book)src;
                return new BookDto(
                    book.Id, book.Title, book.Author, book.Isbn, book.Synopsis, book.CoverUrl,
                    book.PublishedYear, book.OpenLibraryId, book.GoogleBooksId,
                    book.Genres.Select(g => new GenreDto(g.Id, g.Name)).ToList(),
                    book.CreatedAt, book.UpdatedAt);
            });

        _sut = new BookService(_unitOfWorkMock.Object, _externalBookSearchServiceMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsBookDto()
    {
        var book = new Book("Duna", "Frank Herbert", Guid.NewGuid());
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>())).ReturnsAsync(book);

        var result = await _sut.GetByIdAsync(book.Id);

        Assert.NotNull(result);
        Assert.Equal("Duna", result!.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task SearchAsync_ReturnsPagedResult()
    {
        var books = new List<Book> { new("Duna", "Frank Herbert", Guid.NewGuid()) };
        _bookRepositoryMock
            .Setup(r => r.SearchAsync(null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((books, 1));

        var result = await _sut.SearchAsync(new BookSearchQueryDto());

        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public async Task CreateAsync_WithGenreIds_AddsGenresToBook()
    {
        var genre = new Genre("Ficção Científica");
        _genreRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([genre]);

        var request = new CreateBookRequestDto("Duna", "Frank Herbert", null, null, null, null, null, null, [genre.Id]);

        var result = await _sut.CreateAsync(request, Guid.NewGuid());

        Assert.Single(result.Genres);
        _bookRepositoryMock.Verify(r => r.AddAsync(It.Is<Book>(b => b.Genres.Contains(genre)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsNotFoundException()
    {
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateAsync(Guid.NewGuid(), new UpdateBookRequestDto("Duna", "Frank Herbert", null, null, null, null, [])));
    }

    [Fact]
    public async Task UpdateAsync_SyncsGenres_RemovesUnlistedAndAddsNew()
    {
        var oldGenre = new Genre("Aventura");
        var newGenre = new Genre("Ficção Científica");
        var book = new Book("Duna", "Frank Herbert", Guid.NewGuid());
        book.AddGenre(oldGenre);

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>())).ReturnsAsync(book);
        _genreRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.Is<IEnumerable<Guid>>(ids => ids.Contains(newGenre.Id)), It.IsAny<CancellationToken>()))
            .ReturnsAsync([newGenre]);

        var request = new UpdateBookRequestDto("Duna", "Frank Herbert", null, null, null, null, [newGenre.Id]);

        var result = await _sut.UpdateAsync(book.Id, request);

        Assert.Single(result.Genres);
        Assert.Equal(newGenre.Id, result.Genres[0].Id);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ThrowsNotFoundException()
    {
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_Found_RemovesBook()
    {
        var book = new Book("Duna", "Frank Herbert", Guid.NewGuid());
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>())).ReturnsAsync(book);

        await _sut.DeleteAsync(book.Id);

        _bookRepositoryMock.Verify(r => r.Remove(book), Times.Once);
    }

    [Fact]
    public async Task SearchExternalAsync_WithIsbn_CallsSearchByIsbn()
    {
        var externalResult = new ExternalBookResultDto("Duna", "Frank Herbert", "9780441172719", null, null, 1965, null, null, []);
        _externalBookSearchServiceMock
            .Setup(s => s.SearchByIsbnAsync("9780441172719", It.IsAny<CancellationToken>()))
            .ReturnsAsync(externalResult);

        var result = await _sut.SearchExternalAsync(new ExternalBookSearchQueryDto(null, "9780441172719"));

        Assert.Single(result);
        _externalBookSearchServiceMock.Verify(s => s.SearchByTitleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchExternalAsync_WithTitle_CallsSearchByTitle()
    {
        _externalBookSearchServiceMock
            .Setup(s => s.SearchByTitleAsync("Duna", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _sut.SearchExternalAsync(new ExternalBookSearchQueryDto("Duna", null));

        _externalBookSearchServiceMock.Verify(s => s.SearchByTitleAsync("Duna", It.IsAny<CancellationToken>()), Times.Once);
    }
}
