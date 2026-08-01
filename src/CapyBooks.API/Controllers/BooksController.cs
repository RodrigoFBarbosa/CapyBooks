using System.Security.Claims;
using Asp.Versioning;
using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Books;
using CapyBooks.Application.DTOs.Common;
using CapyBooks.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapyBooks.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<BookDto>>> Search([FromQuery] BookSearchQueryDto query, CancellationToken cancellationToken)
        => Ok(await _bookService.SearchAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetByIdAsync(id, cancellationToken);
        return book is null ? NotFound() : Ok(book);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("external-search")]
    public async Task<ActionResult<IReadOnlyList<ExternalBookResultDto>>> SearchExternal(
        [FromQuery] ExternalBookSearchQueryDto query, CancellationToken cancellationToken)
        => Ok(await _bookService.SearchExternalAsync(query, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<BookDto>> Create(CreateBookRequestDto request, CancellationToken cancellationToken)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _bookService.CreateAsync(request, adminId, cancellationToken);
        return Created($"/api/v1/books/{result.Id}", result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BookDto>> Update(Guid id, UpdateBookRequestDto request, CancellationToken cancellationToken)
        => Ok(await _bookService.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _bookService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
