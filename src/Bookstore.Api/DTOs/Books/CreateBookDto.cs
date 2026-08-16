using System.ComponentModel.DataAnnotations;

namespace Bookstore.Api.DTOs.Books;

public sealed class CreateBookDto
{
    [Range(1, int.MaxValue)]
    public int AuthorId { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public required string Title { get; init; }

    [StringLength(200)]
    public string? SubTitle { get; init; }
}
