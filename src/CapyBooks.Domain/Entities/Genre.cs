using CapyBooks.Domain.Common;
using CapyBooks.Domain.Exceptions;

namespace CapyBooks.Domain.Entities;

public class Genre : BaseEntity
{
    private readonly List<Book> _books = [];

    public string Name { get; private set; } = string.Empty;

    public IReadOnlyCollection<Book> Books => _books.AsReadOnly();

    private Genre()
    {
    }

    public Genre(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome do gênero não pode ser vazio.");

        Name = name;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome do gênero não pode ser vazio.");

        Name = name;
    }
}
