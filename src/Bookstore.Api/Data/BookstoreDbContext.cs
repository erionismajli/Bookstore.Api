using Bookstore.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Api.Data;

public sealed class BookstoreDbContext(DbContextOptions<BookstoreDbContext> options)
    : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(author => author.AuthorId);
            entity.Property(author => author.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(author => author.Name);
            entity.HasData(
                new Author { AuthorId = 1, Name = "Ismail Kadare" },
                new Author { AuthorId = 2, Name = "Dritëro Agolli" },
                new Author { AuthorId = 3, Name = "Migjeni" },
                new Author { AuthorId = 4, Name = "Naim Frashëri" });
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(book => book.BookId);
            entity.Property(book => book.Title).HasMaxLength(100).IsRequired();
            entity.Property(book => book.SubTitle).HasMaxLength(200);
            entity.HasIndex(book => book.Title);
            entity.HasOne(book => book.Author)
                .WithMany(author => author.Books)
                .HasForeignKey(book => book.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasData(
                new { BookId = 1, AuthorId = 1, Title = "Gjenerali i ushtrisë së vdekur", SubTitle = (string?)null },
                new { BookId = 2, AuthorId = 1, Title = "Kronikë në gur", SubTitle = (string?)null },
                new { BookId = 3, AuthorId = 1, Title = "Pallati i ëndrrave", SubTitle = (string?)null },
                new { BookId = 4, AuthorId = 2, Title = "Shkëlqimi dhe rënia e shokut Zylo", SubTitle = (string?)null },
                new { BookId = 5, AuthorId = 2, Title = "Komisari Memo", SubTitle = (string?)null },
                new { BookId = 6, AuthorId = 3, Title = "Vargjet e lira", SubTitle = (string?)null },
                new { BookId = 7, AuthorId = 4, Title = "Bagëti e Bujqësia", SubTitle = (string?)null });
        });
    }
}
