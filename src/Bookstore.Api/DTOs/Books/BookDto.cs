using Bookstore.Api.DTOs.Authors;

namespace Bookstore.Api.DTOs.Books;

public sealed record BookDto(int BookId, AuthorDto Author, string Title, string? SubTitle);
