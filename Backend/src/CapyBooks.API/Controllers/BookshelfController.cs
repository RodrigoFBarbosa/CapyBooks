using System.Security.Claims;
using Asp.Versioning;
using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Bookshelves;
using CapyBooks.Application.DTOs.Common;
using CapyBooks.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapyBooks.API.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/bookshelf")]
public class BookshelfController : ControllerBase
{
    private readonly IBookshelfService _bookshelfService;

    public BookshelfController(IBookshelfService bookshelfService)
    {
        _bookshelfService = bookshelfService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<BookshelfDto>>> GetMyShelf(
        [FromQuery] BookshelfSearchQueryDto query, CancellationToken cancellationToken)
        => Ok(await _bookshelfService.GetByUserAsync(CurrentUserId(), query, cancellationToken));

    [HttpGet("{bookId:guid}")]
    public async Task<ActionResult<BookshelfDto>> GetForBook(Guid bookId, CancellationToken cancellationToken)
    {
        var entry = await _bookshelfService.GetByUserAndBookAsync(CurrentUserId(), bookId, cancellationToken);
        return entry is null ? NotFound() : Ok(entry);
    }

    [HttpPut("{bookId:guid}")]
    public async Task<ActionResult<BookshelfDto>> SetStatus(
        Guid bookId, SetBookshelfStatusRequestDto request, CancellationToken cancellationToken)
        => Ok(await _bookshelfService.SetStatusAsync(CurrentUserId(), bookId, request, cancellationToken));

    [HttpDelete("{bookId:guid}")]
    public async Task<IActionResult> Remove(Guid bookId, CancellationToken cancellationToken)
    {
        await _bookshelfService.RemoveAsync(CurrentUserId(), bookId, cancellationToken);
        return NoContent();
    }

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
