using Bookstore.Api.Data;
using Bookstore.Api.DTOs.Authors;
using Bookstore.Api.Entities;
using Bookstore.Api.ErrorHandling;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Api.Services;

public sealed class AuthorService(BookstoreDbContext dbContext, ILogger<AuthorService> logger) : IAuthorService
{
    public async Task<IReadOnlyCollection<AuthorDto>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.Authors.AsNoTracking()
            .OrderBy(author => author.Name)
            .Select(author => new AuthorDto(author.AuthorId, author.Name))
            .ToListAsync(cancellationToken);

    public async Task<AuthorDto> GetByIdAsync(int authorId, CancellationToken cancellationToken)
    {
        var author = await dbContext.Authors.AsNoTracking()
            .SingleOrDefaultAsync(author => author.AuthorId == authorId, cancellationToken)
            ?? throw new NotFoundException($"Author with id {authorId} was not found.");

        return Map(author);
    }

    public async Task<AuthorDto> CreateAsync(CreateAuthorDto request, CancellationToken cancellationToken)
    {
        var author = new Author { Name = request.Name.Trim() };
        dbContext.Authors.Add(author);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created author {AuthorId}", author.AuthorId);
        return Map(author);
    }

    public async Task UpdateAsync(int authorId, UpdateAuthorDto request, CancellationToken cancellationToken)
    {
        var author = await FindAsync(authorId, cancellationToken);
        author.Name = request.Name.Trim();
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Updated author {AuthorId}", authorId);
    }

    public async Task DeleteAsync(int authorId, CancellationToken cancellationToken)
    {
        var author = await FindAsync(authorId, cancellationToken);
        if (await dbContext.Books.AnyAsync(book => book.AuthorId == authorId, cancellationToken))
        {
            throw new ConflictException("An author with books cannot be deleted. Delete or reassign the books first.");
        }

        dbContext.Authors.Remove(author);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Deleted author {AuthorId}", authorId);
    }

    private async Task<Author> FindAsync(int authorId, CancellationToken cancellationToken) =>
        await dbContext.Authors.SingleOrDefaultAsync(author => author.AuthorId == authorId, cancellationToken)
        ?? throw new NotFoundException($"Author with id {authorId} was not found.");

    private static AuthorDto Map(Author author) => new(author.AuthorId, author.Name);
}
