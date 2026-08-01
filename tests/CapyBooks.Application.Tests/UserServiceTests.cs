using AutoMapper;
using CapyBooks.Application.Common.Exceptions;
using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Users;
using CapyBooks.Application.Services;
using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Enums;
using CapyBooks.Domain.Interfaces;
using Moq;

namespace CapyBooks.Application.Tests;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mapperMock.Setup(m => m.Map<UserDto>(It.IsAny<object>()))
            .Returns<object>(src =>
            {
                var user = (User)src;
                return new UserDto(user.Id, user.Name, user.Email, user.Role.ToString(), user.CreatedAt);
            });

        _sut = new UserService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_Found_ReturnsUserDto()
    {
        var user = User.CreateLocal("Rodrigo", "rodrigo@example.com", "hash");
        _userRepositoryMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal("rodrigo@example.com", result!.Email);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task SearchAsync_ReturnsPagedResult()
    {
        var users = new List<User> { User.CreateLocal("Rodrigo", "rodrigo@example.com", "hash") };
        _userRepositoryMock
            .Setup(r => r.SearchAsync(null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((users, 1));

        var result = await _sut.SearchAsync(new UserSearchQueryDto());

        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task ChangeRoleAsync_WithValidRole_UpdatesRole()
    {
        var user = User.CreateLocal("Rodrigo", "rodrigo@example.com", "hash");
        _userRepositoryMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.ChangeRoleAsync(user.Id, new UpdateUserRoleRequestDto("Admin"));

        Assert.Equal("Admin", result.Role);
        Assert.Equal(UserRole.Admin, user.Role);
    }

    [Fact]
    public async Task ChangeRoleAsync_NotFound_ThrowsNotFoundException()
    {
        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.ChangeRoleAsync(Guid.NewGuid(), new UpdateUserRoleRequestDto("Admin")));
    }

    [Fact]
    public async Task DeleteAsync_SelfDelete_ThrowsConflictException()
    {
        var userId = Guid.NewGuid();

        await Assert.ThrowsAsync<ConflictException>(() => _sut.DeleteAsync(userId, userId));

        _userRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ThrowsNotFoundException()
    {
        _userRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_Found_RemovesUser()
    {
        var user = User.CreateLocal("Rodrigo", "rodrigo@example.com", "hash");
        _userRepositoryMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        await _sut.DeleteAsync(user.Id, Guid.NewGuid());

        _userRepositoryMock.Verify(r => r.Remove(user), Times.Once);
    }
}
