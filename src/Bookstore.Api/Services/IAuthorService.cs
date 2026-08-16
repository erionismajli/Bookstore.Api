using Bookstore.Api.DTOs.Authors;

namespace Bookstore.Api.Services;

public interface IAuthorService
{
    Task<IReadOnlyCollection<AuthorDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<AuthorDto> GetByIdAsync(int authorId, CancellationToken cancellationToken);
    Task<AuthorDto> CreateAsync(CreateAuthorDto request, CancellationToken cancellationToken);
    Task UpdateAsync(int authorId, UpdateAuthorDto request, CancellationToken cancellationToken);
    Task DeleteAsync(int authorId, CancellationToken cancellationToken);
}
