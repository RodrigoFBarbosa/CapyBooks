using AutoMapper;
using CapyBooks.Application.Common.Exceptions;
using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Genres;
using CapyBooks.Application.Services;
using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Interfaces;
using Moq;

namespace CapyBooks.Application.Tests;

public class GenreServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IGenreRepository> _genreRepositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly GenreService _sut;

    public GenreServiceTests()
    {
        _unitOfWorkMock.Setup(u => u.Genres).Returns(_genreRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mapperMock.Setup(m => m.Map<GenreDto>(It.IsAny<object>()))
            .Returns<object>(src =>
            {
                var genre = (Genre)src;
                return new GenreDto(genre.Id, genre.Name);
            });

        _sut = new GenreService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedGenres()
    {
        var genres = new List<Genre> { new("Fantasia"), new("Terror") };
        _genreRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(genres);

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task CreateAsync_WithNewName_CreatesGenre()
    {
        _genreRepositoryMock.Setup(r => r.GetByNameAsync("Fantasia", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Genre?)null);

        var result = await _sut.CreateAsync(new CreateGenreRequestDto("Fantasia"));

        Assert.Equal("Fantasia", result.Name);
        _genreRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Genre>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ThrowsConflictException()
    {
        _genreRepositoryMock.Setup(r => r.GetByNameAsync("Fantasia", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Genre("Fantasia"));

        await Assert.ThrowsAsync<ConflictException>(() => _sut.CreateAsync(new CreateGenreRequestDto("Fantasia")));
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsNotFoundException()
    {
        _genreRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Genre?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateAsync(Guid.NewGuid(), new UpdateGenreRequestDto("Terror")));
    }

    [Fact]
    public async Task UpdateAsync_WithValidNewName_RenamesGenre()
    {
        var genre = new Genre("Fantasia");
        _genreRepositoryMock.Setup(r => r.GetByIdAsync(genre.Id, It.IsAny<CancellationToken>())).ReturnsAsync(genre);
        _genreRepositoryMock.Setup(r => r.GetByNameAsync("Terror", It.IsAny<CancellationToken>())).ReturnsAsync((Genre?)null);

        var result = await _sut.UpdateAsync(genre.Id, new UpdateGenreRequestDto("Terror"));

        Assert.Equal("Terror", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_WithNameTakenByAnotherGenre_ThrowsConflictException()
    {
        var genre = new Genre("Fantasia");
        var otherGenre = new Genre("Terror");
        _genreRepositoryMock.Setup(r => r.GetByIdAsync(genre.Id, It.IsAny<CancellationToken>())).ReturnsAsync(genre);
        _genreRepositoryMock.Setup(r => r.GetByNameAsync("Terror", It.IsAny<CancellationToken>())).ReturnsAsync(otherGenre);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.UpdateAsync(genre.Id, new UpdateGenreRequestDto("Terror")));
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ThrowsNotFoundException()
    {
        _genreRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Genre?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_Found_RemovesGenre()
    {
        var genre = new Genre("Fantasia");
        _genreRepositoryMock.Setup(r => r.GetByIdAsync(genre.Id, It.IsAny<CancellationToken>())).ReturnsAsync(genre);

        await _sut.DeleteAsync(genre.Id);

        _genreRepositoryMock.Verify(r => r.Remove(genre), Times.Once);
    }
}
