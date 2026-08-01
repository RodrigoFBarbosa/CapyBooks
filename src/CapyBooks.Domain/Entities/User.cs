using CapyBooks.Domain.Common;
using CapyBooks.Domain.Enums;
using CapyBooks.Domain.Exceptions;

namespace CapyBooks.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PasswordHash { get; private set; }
    public string? GoogleId { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User()
    {
    }

    private User(string name, string email, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome do usuário não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("O e-mail do usuário não pode ser vazio.");

        Name = name;
        Email = email;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    public static User CreateLocal(string name, string email, string passwordHash, UserRole role = UserRole.User)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("O hash de senha não pode ser vazio.");

        return new User(name, email, role) { PasswordHash = passwordHash };
    }

    public static User CreateFromGoogle(string name, string email, string googleId, UserRole role = UserRole.User)
    {
        if (string.IsNullOrWhiteSpace(googleId))
            throw new DomainException("O identificador do Google não pode ser vazio.");

        return new User(name, email, role) { GoogleId = googleId };
    }

    public void UpdatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("O hash de senha não pode ser vazio.");

        PasswordHash = passwordHash;
    }

    public void LinkGoogleAccount(string googleId)
    {
        if (string.IsNullOrWhiteSpace(googleId))
            throw new DomainException("O identificador do Google não pode ser vazio.");

        GoogleId = googleId;
    }

    public void UpdateProfile(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome do usuário não pode ser vazio.");

        Name = name;
    }

    public void ChangeRole(UserRole role)
    {
        Role = role;
    }
}
