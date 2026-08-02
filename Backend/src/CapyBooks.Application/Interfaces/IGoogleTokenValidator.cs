namespace CapyBooks.Application.Interfaces;

public interface IGoogleTokenValidator
{
    Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}

public record GoogleUserInfo(string GoogleId, string Email, string Name);
