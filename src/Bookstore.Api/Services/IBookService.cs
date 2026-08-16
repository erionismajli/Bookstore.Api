using Bookstore.Api.DTOs;
using Bookstore.Api.DTOs.Books;

namespace Bookstore.Api.Services;

public interface IBookService
{
    Task<IReadOnlyCollection<BookDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<BookDto> GetByIdAsync(int bookId, CancellationToken cancellationToken);
    Task<BookDto> CreateAsync(CreateBookDto request, CancellationToken cancellationToken);
    Task UpdateAsync(int bookId, UpdateBookDto request, CancellationToken cancellationToken);
    Task DeleteAsync(int bookId, CancellationToken cancellationToken);
    Task<PagedResultDto<BookDto>> SearchAsync(
        string? title,
        string? author,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
