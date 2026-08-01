using CapyBooks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CapyBooks.Infrastructure.Persistence.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(b => b.Isbn)
            .HasMaxLength(20);

        builder.Property(b => b.CoverUrl)
            .HasMaxLength(1000);

        builder.Property(b => b.OpenLibraryId)
            .HasMaxLength(50);

        builder.Property(b => b.GoogleBooksId)
            .HasMaxLength(50);

        builder.Property(b => b.CreatedByAdminId)
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.HasIndex(b => b.Isbn)
            .IsUnique()
            .HasFilter("isbn IS NOT NULL");

        // Restrict: remover um admin não pode apagar em cascata todo o catálogo criado por ele.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.CreatedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Genres)
            .WithMany(g => g.Books)
            .UsingEntity(j => j.ToTable("book_genres"));

        builder.Navigation(b => b.Genres)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
