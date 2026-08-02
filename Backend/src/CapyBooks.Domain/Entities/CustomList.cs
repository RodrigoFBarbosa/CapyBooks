using CapyBooks.Domain.Common;
using CapyBooks.Domain.Exceptions;

namespace CapyBooks.Domain.Entities;

public class CustomList : BaseEntity
{
    private readonly List<ListItem> _items = [];

    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public IReadOnlyCollection<ListItem> Items => _items.AsReadOnly();

    private CustomList()
    {
    }

    public CustomList(Guid userId, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome da lista não pode ser vazio.");

        UserId = userId;
        Name = name;
        Description = description;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome da lista não pode ser vazio.");

        Name = name;
    }

    public void UpdateDescription(string? description) => Description = description;

    public void AddBook(Guid bookId)
    {
        if (_items.Any(i => i.BookId == bookId))
            throw new DomainException("Este livro já está na lista.");

        var nextOrder = _items.Count == 0 ? 0 : _items.Max(i => i.Order) + 1;
        _items.Add(new ListItem(Id, bookId, nextOrder));
    }

    public void RemoveBook(Guid bookId)
    {
        var item = _items.FirstOrDefault(i => i.BookId == bookId);
        if (item is null)
            return;

        _items.Remove(item);
        Reindex();
    }

    public void ReorderBook(Guid bookId, int newIndex)
    {
        if (newIndex < 0 || newIndex >= _items.Count)
            throw new DomainException("Posição inválida para reordenar a lista.");

        var item = _items.FirstOrDefault(i => i.BookId == bookId)
            ?? throw new DomainException("Este livro não está na lista.");

        _items.Remove(item);
        _items.Insert(newIndex, item);
        Reindex();
    }

    private void Reindex()
    {
        for (var i = 0; i < _items.Count; i++)
            _items[i].UpdateOrder(i);
    }
}
