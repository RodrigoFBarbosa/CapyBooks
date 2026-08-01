using CapyBooks.Domain.Common;
using CapyBooks.Domain.Exceptions;

namespace CapyBooks.Domain.Entities;

public class ReadingLink : BaseEntity
{
    public Guid BookId { get; private set; }
    public string SourceName { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;

    private ReadingLink()
    {
    }

    public ReadingLink(Guid bookId, string sourceName, string url)
    {
        if (string.IsNullOrWhiteSpace(sourceName))
            throw new DomainException("O nome da fonte não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("A URL não pode ser vazia.");

        BookId = bookId;
        SourceName = sourceName;
        Url = url;
    }

    public void Update(string sourceName, string url)
    {
        if (string.IsNullOrWhiteSpace(sourceName))
            throw new DomainException("O nome da fonte não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(url))
            throw new DomainException("A URL não pode ser vazia.");

        SourceName = sourceName;
        Url = url;
    }
}
