using Bookstore.Api.DTOs.Authors;
using Bookstore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class AuthorsController : ControllerBase
{
    private readonly IAuthorService _authorService;

    public AuthorsController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<AuthorDto>>> GetAll(CancellationToken cancellationToken)
    {
        var authors = await _authorService.GetAllAsync(cancellationToken);

        return Ok(authors);
    }

    [HttpGet("{authorId:int}")]
    public async Task<ActionResult<AuthorDto>> GetById(int authorId, CancellationToken cancellationToken)
    {
        var author = await _authorService.GetByIdAsync(authorId, cancellationToken);

        return Ok(author);
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> Create(CreateAuthorDto request, CancellationToken cancellationToken)
    {
        var author = await _authorService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { authorId = author.AuthorId }, author);
    }

    [HttpPut("{authorId:int}")]
    public async Task<IActionResult> Update(int authorId, UpdateAuthorDto request, CancellationToken cancellationToken)
    {
        await _authorService.UpdateAsync(authorId, request, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{authorId:int}")]
    public async Task<IActionResult> Delete(int authorId, CancellationToken cancellationToken)
    {
        await _authorService.DeleteAsync(authorId, cancellationToken);

        return NoContent();
    }
}
