using System.ComponentModel.DataAnnotations;

namespace Bookstore.Api.DTOs.Authors;

public sealed class CreateAuthorDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public required string Name { get; init; }
}
