using CapyBooks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CapyBooks.Infrastructure.Persistence.Configurations;

public class ListItemConfiguration : IEntityTypeConfiguration<ListItem>
{
    public void Configure(EntityTypeBuilder<ListItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Order)
            .IsRequired();

        builder.HasOne<Book>()
            .WithMany()
            .HasForeignKey(i => i.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => new { i.CustomListId, i.BookId })
            .IsUnique();
    }
}
