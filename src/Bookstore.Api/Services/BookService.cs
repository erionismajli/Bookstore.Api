using Bookstore.Api.Data;
using Bookstore.Api.DTOs;
using Bookstore.Api.DTOs.Authors;
using Bookstore.Api.DTOs.Books;
using Bookstore.Api.Entities;
using Bookstore.Api.ErrorHandling;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Api.Services;

public sealed class BookService(BookstoreDbContext dbContext, ILogger<BookService> logger) : IBookService
{
    public async Task<IReadOnlyCollection<BookDto>> GetAllAsync(CancellationToken cancellationToken) =>
        await Project(dbContext.Books.AsNoTracking()
            .OrderBy(book => book.Title)
            .ThenBy(book => book.BookId))
            .ToListAsync(cancellationToken);

    public async Task<BookDto> GetByIdAsync(int bookId, CancellationToken cancellationToken) =>
        await Project(dbContext.Books.AsNoTracking().Where(book => book.BookId == bookId))
            .SingleOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException($"Book with id {bookId} was not found.");

    public async Task<BookDto> CreateAsync(CreateBookDto request, CancellationToken cancellationToken)
    {
        var author = await GetAuthorAsync(request.AuthorId, cancellationToken);
        var book = new Book
        {
            AuthorId = author.AuthorId,
            Author = author,
            Title = request.Title.Trim(),
            SubTitle = NormalizeOptional(request.SubTitle)
        };

        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created book {BookId} for author {AuthorId}", book.BookId, author.AuthorId);

        return Map(book);
    }

    public async Task UpdateAsync(int bookId, UpdateBookDto request, CancellationToken cancellationToken)
    {
        var book = await dbContext.Books.SingleOrDefaultAsync(book => book.BookId == bookId, cancellationToken)
            ?? throw new NotFoundException($"Book with id {bookId} was not found.");
        await GetAuthorAsync(request.AuthorId, cancellationToken);

        book.AuthorId = request.AuthorId;
        book.Title = request.Title.Trim();
        book.SubTitle = NormalizeOptional(request.SubTitle);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated book {BookId}", bookId);
    }

    public async Task DeleteAsync(int bookId, CancellationToken cancellationToken)
    {
        var book = await dbContext.Books.SingleOrDefaultAsync(book => book.BookId == bookId, cancellationToken)
            ?? throw new NotFoundException($"Book with id {bookId} was not found.");

        dbContext.Books.Remove(book);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Deleted book {BookId}", bookId);
    }

    public async Task<PagedResultDto<BookDto>> SearchAsync(
        string? title,
        string? author,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Books.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
        {
            var titleTerm = title.Trim();
            query = query.Where(book => book.Title.Contains(titleTerm));
        }

        if (!string.IsNullOrWhiteSpace(author))
        {
            var authorTerm = author.Trim();
            query = query.Where(book => book.Author.Name.Contains(authorTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var pagedQuery = query
            .OrderBy(book => book.Title)
            .ThenBy(book => book.BookId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
        var items = await Project(pagedQuery)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<BookDto>(items, page, pageSize, totalCount);
    }

    private async Task<Author> GetAuthorAsync(int authorId, CancellationToken cancellationToken) =>
        await dbContext.Authors.FindAsync([authorId], cancellationToken)
        ?? throw new NotFoundException($"Author with id {authorId} was not found.");

    private static IQueryable<BookDto> Project(IQueryable<Book> query) =>
        query.Select(book => new BookDto(
            book.BookId,
            new AuthorDto(book.Author.AuthorId, book.Author.Name),
            book.Title,
            book.SubTitle));

    private static BookDto Map(Book book) =>
        new(book.BookId, new AuthorDto(book.Author.AuthorId, book.Author.Name), book.Title, book.SubTitle);

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
