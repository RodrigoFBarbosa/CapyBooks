using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Exceptions;

namespace CapyBooks.Domain.Tests;

public class RefreshTokenTests
{
    [Fact]
    public void IsActive_WhenNotRevokedAndNotExpired_ReturnsTrue()
    {
        var token = new RefreshToken(Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(1));

        Assert.True(token.IsActive);
    }

    [Fact]
    public void IsActive_WhenExpired_ReturnsFalse()
    {
        var token = new RefreshToken(Guid.NewGuid(), "hash", DateTime.UtcNow.AddSeconds(-1));

        Assert.False(token.IsActive);
    }

    [Fact]
    public void IsActive_WhenRevoked_ReturnsFalse()
    {
        var token = new RefreshToken(Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(1));

        token.Revoke();

        Assert.False(token.IsActive);
    }

    [Fact]
    public void Revoke_WithReplacement_SetsReplacedByTokenHash()
    {
        var token = new RefreshToken(Guid.NewGuid(), "hash", DateTime.UtcNow.AddDays(1));

        token.Revoke("new-hash");

        Assert.Equal("new-hash", token.ReplacedByTokenHash);
        Assert.NotNull(token.RevokedAt);
    }

    [Fact]
    public void Constructor_WithEmptyTokenHash_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => new RefreshToken(Guid.NewGuid(), " ", DateTime.UtcNow.AddDays(1)));
    }
}
