using System.ComponentModel.DataAnnotations;
using Bookstore.Api.Configuration;
using Bookstore.Api.DTOs;
using Bookstore.Api.DTOs.Books;
using Bookstore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    [Authorize(Policy = AuthExtensions.BookCrudPolicy)]
    public async Task<ActionResult<IReadOnlyCollection<BookDto>>> GetAll(CancellationToken cancellationToken)
    {
        var books = await _bookService.GetAllAsync(cancellationToken);

        return Ok(books);
    }

    [HttpGet("{bookId:int}")]
    [Authorize(Policy = AuthExtensions.BookCrudPolicy)]
    public async Task<ActionResult<BookDto>> GetById(int bookId, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetByIdAsync(bookId, cancellationToken);

        return Ok(book);
    }

    [HttpGet("search")]
    [Authorize(Policy = AuthExtensions.BookSearchPolicy)]
    public async Task<ActionResult<PagedResultDto<BookDto>>> Search(
        [FromQuery] string? title,
        [FromQuery] string? author,
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _bookService.SearchAsync(title, author, page, pageSize, cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthExtensions.BookCrudPolicy)]
    public async Task<ActionResult<BookDto>> Create(CreateBookDto request, CancellationToken cancellationToken)
    {
        var book = await _bookService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { bookId = book.BookId }, book);
    }

    [HttpPut("{bookId:int}")]
    [Authorize(Policy = AuthExtensions.BookCrudPolicy)]
    public async Task<IActionResult> Update(int bookId, UpdateBookDto request, CancellationToken cancellationToken)
    {
        await _bookService.UpdateAsync(bookId, request, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{bookId:int}")]
    [Authorize(Policy = AuthExtensions.BookCrudPolicy)]
    public async Task<IActionResult> Delete(int bookId, CancellationToken cancellationToken)
    {
        await _bookService.DeleteAsync(bookId, cancellationToken);

        return NoContent();
    }
}
