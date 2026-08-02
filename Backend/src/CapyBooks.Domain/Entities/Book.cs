using CapyBooks.Domain.Common;
using CapyBooks.Domain.Exceptions;

namespace CapyBooks.Domain.Entities;

public class Book : BaseEntity
{
    private readonly List<Genre> _genres = [];

    public string Title { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public string? Isbn { get; private set; }
    public string? Synopsis { get; private set; }
    public string? CoverUrl { get; private set; }
    public int? PublishedYear { get; private set; }
    public string? OpenLibraryId { get; private set; }
    public string? GoogleBooksId { get; private set; }
    public Guid CreatedByAdminId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<Genre> Genres => _genres.AsReadOnly();

    private Book()
    {
    }

    public Book(
        string title,
        string author,
        Guid createdByAdminId,
        string? isbn = null,
        string? synopsis = null,
        string? coverUrl = null,
        int? publishedYear = null,
        string? openLibraryId = null,
        string? googleBooksId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("O título do livro não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(author))
            throw new DomainException("O autor do livro não pode ser vazio.");

        Title = title;
        Author = author;
        CreatedByAdminId = createdByAdminId;
        Isbn = isbn;
        Synopsis = synopsis;
        CoverUrl = coverUrl;
        PublishedYear = publishedYear;
        OpenLibraryId = openLibraryId;
        GoogleBooksId = googleBooksId;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(
        string title,
        string author,
        string? isbn,
        string? synopsis,
        string? coverUrl,
        int? publishedYear)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("O título do livro não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(author))
            throw new DomainException("O autor do livro não pode ser vazio.");

        Title = title;
        Author = author;
        Isbn = isbn;
        Synopsis = synopsis;
        CoverUrl = coverUrl;
        PublishedYear = publishedYear;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddGenre(Genre genre)
    {
        if (!_genres.Contains(genre))
            _genres.Add(genre);
    }

    public void RemoveGenre(Genre genre) => _genres.Remove(genre);
}
