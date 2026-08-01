using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Enums;
using CapyBooks.Domain.Exceptions;

namespace CapyBooks.Domain.Tests;

public class UserTests
{
    [Fact]
    public void CreateLocal_WithValidData_SetsPasswordHashAndNoGoogleId()
    {
        var user = User.CreateLocal("Rodrigo", "rodrigo@example.com", "hash");

        Assert.Equal("hash", user.PasswordHash);
        Assert.Null(user.GoogleId);
        Assert.Equal(UserRole.User, user.Role);
    }

    [Fact]
    public void CreateLocal_WithEmptyPasswordHash_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => User.CreateLocal("Rodrigo", "rodrigo@example.com", " "));
    }

    [Fact]
    public void CreateFromGoogle_WithValidData_SetsGoogleIdAndNoPasswordHash()
    {
        var user = User.CreateFromGoogle("Rodrigo", "rodrigo@example.com", "google-123");

        Assert.Equal("google-123", user.GoogleId);
        Assert.Null(user.PasswordHash);
    }

    [Fact]
    public void CreateFromGoogle_WithEmptyGoogleId_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => User.CreateFromGoogle("Rodrigo", "rodrigo@example.com", " "));
    }

    [Fact]
    public void LinkGoogleAccount_SetsGoogleId()
    {
        var user = User.CreateLocal("Rodrigo", "rodrigo@example.com", "hash");

        user.LinkGoogleAccount("google-123");

        Assert.Equal("google-123", user.GoogleId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithInvalidName_ThrowsDomainException(string name)
    {
        Assert.Throws<DomainException>(() => User.CreateLocal(name, "rodrigo@example.com", "hash"));
    }
}
