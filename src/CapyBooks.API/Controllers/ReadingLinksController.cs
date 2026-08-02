using Asp.Versioning;
using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.ReadingLinks;
using CapyBooks.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapyBooks.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/books/{bookId:guid}/reading-links")]
public class ReadingLinksController : ControllerBase
{
    private readonly IReadingLinkService _readingLinkService;

    public ReadingLinksController(IReadingLinkService readingLinkService)
    {
        _readingLinkService = readingLinkService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReadingLinkDto>>> GetByBook(Guid bookId, CancellationToken cancellationToken)
        => Ok(await _readingLinkService.GetByBookAsync(bookId, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ReadingLinkDto>> Create(Guid bookId, CreateReadingLinkRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _readingLinkService.CreateAsync(bookId, request, cancellationToken);
        return Created($"/api/v1/reading-links/{result.Id}", result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("~/api/v{version:apiVersion}/reading-links/{id:guid}")]
    public async Task<ActionResult<ReadingLinkDto>> Update(Guid id, UpdateReadingLinkRequestDto request, CancellationToken cancellationToken)
        => Ok(await _readingLinkService.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpDelete("~/api/v{version:apiVersion}/reading-links/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _readingLinkService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
