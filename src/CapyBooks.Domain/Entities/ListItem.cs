using CapyBooks.Domain.Common;

namespace CapyBooks.Domain.Entities;

public class ListItem : BaseEntity
{
    public Guid CustomListId { get; private set; }
    public Guid BookId { get; private set; }
    public int Order { get; private set; }

    private ListItem()
    {
    }

    internal ListItem(Guid customListId, Guid bookId, int order)
    {
        CustomListId = customListId;
        BookId = bookId;
        Order = order;
    }

    internal void UpdateOrder(int order) => Order = order;
}
