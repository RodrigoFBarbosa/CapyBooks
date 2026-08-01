using CapyBooks.Domain.Common;
using CapyBooks.Domain.Enums;
using CapyBooks.Domain.Exceptions;

namespace CapyBooks.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User()
    {
    }

    public User(string name, string email, string passwordHash, UserRole role = UserRole.User)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome do usuário não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("O e-mail do usuário não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("O hash de senha não pode ser vazio.");

        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("O hash de senha não pode ser vazio.");

        PasswordHash = passwordHash;
    }

    public void UpdateProfile(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome do usuário não pode ser vazio.");

        Name = name;
    }
}
