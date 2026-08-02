using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Exceptions;

namespace CapyBooks.Domain.Tests;

public class CustomListTests
{
    [Fact]
    public void AddBook_AssignsSequentialOrder()
    {
        var list = new CustomList(Guid.NewGuid(), "Favoritos");
        var book1 = Guid.NewGuid();
        var book2 = Guid.NewGuid();

        list.AddBook(book1);
        list.AddBook(book2);

        Assert.Equal(2, list.Items.Count);
        Assert.Equal(0, list.Items.Single(i => i.BookId == book1).Order);
        Assert.Equal(1, list.Items.Single(i => i.BookId == book2).Order);
    }

    [Fact]
    public void AddBook_Duplicate_ThrowsDomainException()
    {
        var list = new CustomList(Guid.NewGuid(), "Favoritos");
        var bookId = Guid.NewGuid();
        list.AddBook(bookId);

        Assert.Throws<DomainException>(() => list.AddBook(bookId));
    }

    [Fact]
    public void RemoveBook_ReindexesRemainingItems()
    {
        var list = new CustomList(Guid.NewGuid(), "Favoritos");
        var book1 = Guid.NewGuid();
        var book2 = Guid.NewGuid();
        var book3 = Guid.NewGuid();
        list.AddBook(book1);
        list.AddBook(book2);
        list.AddBook(book3);

        list.RemoveBook(book1);

        Assert.Equal(2, list.Items.Count);
        Assert.Equal(0, list.Items.Single(i => i.BookId == book2).Order);
        Assert.Equal(1, list.Items.Single(i => i.BookId == book3).Order);
    }

    [Fact]
    public void ReorderBook_MovesItemToNewPosition()
    {
        var list = new CustomList(Guid.NewGuid(), "Favoritos");
        var book1 = Guid.NewGuid();
        var book2 = Guid.NewGuid();
        var book3 = Guid.NewGuid();
        list.AddBook(book1);
        list.AddBook(book2);
        list.AddBook(book3);

        list.ReorderBook(book3, 0);

        Assert.Equal([book3, book1, book2], list.Items.OrderBy(i => i.Order).Select(i => i.BookId));
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new CustomList(Guid.NewGuid(), " "));
    }
}
