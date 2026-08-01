using CapyBooks.Domain.Common;
using CapyBooks.Domain.Enums;

namespace CapyBooks.Domain.Entities;

public class Bookshelf : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid BookId { get; private set; }
    public BookshelfStatus Status { get; private set; }

    private Bookshelf()
    {
    }

    public Bookshelf(Guid userId, Guid bookId, BookshelfStatus status)
    {
        UserId = userId;
        BookId = bookId;
        Status = status;
    }

    public void UpdateStatus(BookshelfStatus status) => Status = status;
}
