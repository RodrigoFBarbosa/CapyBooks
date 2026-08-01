using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Exceptions;

namespace CapyBooks.Domain.Tests;

public class GenreTests
{
    [Fact]
    public void Constructor_WithValidName_CreatesGenre()
    {
        var genre = new Genre("Fantasia");

        Assert.Equal("Fantasia", genre.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithEmptyName_ThrowsDomainException(string name)
    {
        Assert.Throws<DomainException>(() => new Genre(name));
    }

    [Fact]
    public void Rename_WithValidName_UpdatesName()
    {
        var genre = new Genre("Fantasia");

        genre.Rename("Ficção Científica");

        Assert.Equal("Ficção Científica", genre.Name);
    }

    [Fact]
    public void Rename_WithEmptyName_ThrowsDomainException()
    {
        var genre = new Genre("Fantasia");

        Assert.Throws<DomainException>(() => genre.Rename(" "));
    }
}
