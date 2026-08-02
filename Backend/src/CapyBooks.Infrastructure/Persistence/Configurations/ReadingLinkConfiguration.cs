using CapyBooks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CapyBooks.Infrastructure.Persistence.Configurations;

public class ReadingLinkConfiguration : IEntityTypeConfiguration<ReadingLink>
{
    public void Configure(EntityTypeBuilder<ReadingLink> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.SourceName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Url)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasOne<Book>()
            .WithMany()
            .HasForeignKey(r => r.BookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
