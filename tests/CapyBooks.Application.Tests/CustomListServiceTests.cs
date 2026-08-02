using CapyBooks.Application.Common.Exceptions;
using CapyBooks.Application.DTOs.CustomLists;
using CapyBooks.Application.Services;
using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Exceptions;
using CapyBooks.Domain.Interfaces;
using Moq;

namespace CapyBooks.Application.Tests;

public class CustomListServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IBookRepository> _bookRepositoryMock = new();
    private readonly Mock<ICustomListRepository> _customListRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly CustomListService _sut;

    public CustomListServiceTests()
    {
        _unitOfWorkMock.Setup(u => u.Books).Returns(_bookRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CustomLists).Returns(_customListRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _bookRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _sut = new CustomListService(_unitOfWorkMock.Object);
    }

    private User SetUpOwner(Guid? id = null)
    {
        var owner = User.CreateLocal("Rodrigo", $"{Guid.NewGuid()}@example.com", "hash");
        _userRepositoryMock.Setup(r => r.GetByIdAsync(owner.Id, It.IsAny<CancellationToken>())).ReturnsAsync(owner);
        _userRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([owner]);
        return owner;
    }

    [Fact]
    public async Task SearchAsync_ReturnsPagedResultWithUserNameAndItemCount()
    {
        var owner = SetUpOwner();
        var list = new CustomList(owner.Id, "Favoritos");
        list.AddBook(Guid.NewGuid());

        _customListRepositoryMock
            .Setup(r => r.SearchAsync(null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(([list], 1));

        var result = await _sut.SearchAsync(new CustomListSearchQueryDto());

        Assert.Single(result.Items);
        Assert.Equal("Rodrigo", result.Items[0].UserName);
        Assert.Equal(1, result.Items[0].ItemCount);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        _customListRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomList?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_CreatesListOwnedByUser()
    {
        var owner = SetUpOwner();

        var result = await _sut.CreateAsync(owner.Id, new CreateCustomListRequestDto("Favoritos", "Meus livros favoritos"));

        Assert.Equal("Favoritos", result.Name);
        Assert.Equal(owner.Id, result.UserId);
        _customListRepositoryMock.Verify(r => r.AddAsync(It.IsAny<CustomList>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotOwner_ThrowsForbiddenException()
    {
        var list = new CustomList(Guid.NewGuid(), "Favoritos");
        _customListRepositoryMock.Setup(r => r.GetByIdAsync(list.Id, It.IsAny<CancellationToken>())).ReturnsAsync(list);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.UpdateAsync(list.Id, Guid.NewGuid(), new UpdateCustomListRequestDto("Outro nome", null)));
    }

    [Fact]
    public async Task UpdateAsync_Owner_UpdatesNameAndDescription()
    {
        var owner = SetUpOwner();
        var list = new CustomList(owner.Id, "Favoritos");
        _customListRepositoryMock.Setup(r => r.GetByIdAsync(list.Id, It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var result = await _sut.UpdateAsync(list.Id, owner.Id, new UpdateCustomListRequestDto("Clássicos", "Livros clássicos"));

        Assert.Equal("Clássicos", result.Name);
        Assert.Equal("Livros clássicos", result.Description);
    }

    [Fact]
    public async Task DeleteAsync_NotOwner_ThrowsForbiddenException()
    {
        var list = new CustomList(Guid.NewGuid(), "Favoritos");
        _customListRepositoryMock.Setup(r => r.GetByIdAsync(list.Id, It.IsAny<CancellationToken>())).ReturnsAsync(list);

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.DeleteAsync(list.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_Owner_RemovesList()
    {
        var ownerId = Guid.NewGuid();
        var list = new CustomList(ownerId, "Favoritos");
        _customListRepositoryMock.Setup(r => r.GetByIdAsync(list.Id, It.IsAny<CancellationToken>())).ReturnsAsync(list);

        await _sut.DeleteAsync(list.Id, ownerId);

        _customListRepositoryMock.Verify(r => r.Remove(list), Times.Once);
    }

    [Fact]
    public async Task AddItemAsync_BookNotFound_ThrowsNotFoundException()
    {
        var owner = SetUpOwner();
        var list = new CustomList(owner.Id, "Favoritos");
        _customListRepositoryMock.Setup(r => r.GetByIdAsync(list.Id, It.IsAny<CancellationToken>())).ReturnsAsync(list);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Book?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.AddItemAsync(list.Id, owner.Id, new AddListItemRequestDto(Guid.NewGuid())));
    }

    [Fact]
    public async Task AddItemAsync_Valid_AddsItemToList()
    {
        var owner = SetUpOwner();
        var list = new CustomList(owner.Id, "Favoritos");
        var book = new Book("Duna", "Frank Herbert", Guid.NewGuid());

        _customListRepositoryMock.Setup(r => r.GetByIdAsync(list.Id, It.IsAny<CancellationToken>())).ReturnsAsync(list);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>())).ReturnsAsync(book);
        _bookRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([book]);

        var result = await _sut.AddItemAsync(list.Id, owner.Id, new AddListItemRequestDto(book.Id));

        Assert.Single(result.Items);
        Assert.Equal("Duna", result.Items[0].BookTitle);
        Assert.Equal(0, result.Items[0].Order);
    }

    [Fact]
    public async Task AddItemAsync_DuplicateBook_ThrowsDomainException()
    {
        var owner = SetUpOwner();
        var book = new Book("Duna", "Frank Herbert", Guid.NewGuid());
        var list = new CustomList(owner.Id, "Favoritos");
        list.AddBook(book.Id);

        _customListRepositoryMock.Setup(r => r.GetByIdAsync(list.Id, It.IsAny<CancellationToken>())).ReturnsAsync(list);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>())).ReturnsAsync(book);

        await Assert.ThrowsAsync<DomainException>(() =>
            _sut.AddItemAsync(list.Id, owner.Id, new AddListItemRequestDto(book.Id)));
    }

    [Fact]
    public async Task RemoveItemAsync_Owner_RemovesItemFromList()
    {
        var owner = SetUpOwner();
        var bookId = Guid.NewGuid();
        var list = new CustomList(owner.Id, "Favoritos");
        list.AddBook(bookId);

        _customListRepositoryMock.Setup(r => r.GetByIdAsync(list.Id, It.IsAny<CancellationToken>())).ReturnsAsync(list);
        _bookRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _sut.RemoveItemAsync(list.Id, owner.Id, bookId);

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task ReorderItemAsync_Owner_ReordersItems()
    {
        var owner = SetUpOwner();
        var book1 = Guid.NewGuid();
        var book2 = Guid.NewGuid();
        var list = new CustomList(owner.Id, "Favoritos");
        list.AddBook(book1);
        list.AddBook(book2);

        _customListRepositoryMock.Setup(r => r.GetByIdAsync(list.Id, It.IsAny<CancellationToken>())).ReturnsAsync(list);
        _bookRepositoryMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _sut.ReorderItemAsync(list.Id, owner.Id, book2, new ReorderListItemRequestDto(0));

        Assert.Equal(book2, result.Items[0].BookId);
        Assert.Equal(book1, result.Items[1].BookId);
    }

    [Fact]
    public async Task ReorderItemAsync_NotOwner_ThrowsForbiddenException()
    {
        var list = new CustomList(Guid.NewGuid(), "Favoritos");
        _customListRepositoryMock.Setup(r => r.GetByIdAsync(list.Id, It.IsAny<CancellationToken>())).ReturnsAsync(list);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _sut.ReorderItemAsync(list.Id, Guid.NewGuid(), Guid.NewGuid(), new ReorderListItemRequestDto(0)));
    }
}
