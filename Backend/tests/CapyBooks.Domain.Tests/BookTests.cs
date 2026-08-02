using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Exceptions;

namespace CapyBooks.Domain.Tests;

public class BookTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesBook()
    {
        var adminId = Guid.NewGuid();
        var book = new Book("Duna", "Frank Herbert", adminId, isbn: "9780441172719", publishedYear: 1965);

        Assert.Equal("Duna", book.Title);
        Assert.Equal("Frank Herbert", book.Author);
        Assert.Equal(adminId, book.CreatedByAdminId);
        Assert.Empty(book.Genres);
    }

    [Theory]
    [InlineData("", "Frank Herbert")]
    [InlineData("Duna", "")]
    public void Constructor_WithEmptyTitleOrAuthor_ThrowsDomainException(string title, string author)
    {
        Assert.Throws<DomainException>(() => new Book(title, author, Guid.NewGuid()));
    }

    [Fact]
    public void Update_SetsFieldsAndUpdatedAt()
    {
        var book = new Book("Duna", "Frank Herbert", Guid.NewGuid());

        book.Update("Duna (edição especial)", "Frank Herbert", "9780441172719", "Sinopse ajustada", "http://cover.jpg", 1965);

        Assert.Equal("Duna (edição especial)", book.Title);
        Assert.Equal("Sinopse ajustada", book.Synopsis);
        Assert.NotNull(book.UpdatedAt);
    }

    [Fact]
    public void AddGenre_DoesNotAddDuplicate()
    {
        var book = new Book("Duna", "Frank Herbert", Guid.NewGuid());
        var genre = new Genre("Ficção Científica");

        book.AddGenre(genre);
        book.AddGenre(genre);

        Assert.Single(book.Genres);
    }

    [Fact]
    public void RemoveGenre_RemovesExistingGenre()
    {
        var book = new Book("Duna", "Frank Herbert", Guid.NewGuid());
        var genre = new Genre("Ficção Científica");
        book.AddGenre(genre);

        book.RemoveGenre(genre);

        Assert.Empty(book.Genres);
    }
}
