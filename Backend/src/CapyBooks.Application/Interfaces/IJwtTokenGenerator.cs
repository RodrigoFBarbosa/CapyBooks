using CapyBooks.Domain.Entities;

namespace CapyBooks.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);

    (string Token, DateTime ExpiresAt) GenerateRefreshToken();
}
