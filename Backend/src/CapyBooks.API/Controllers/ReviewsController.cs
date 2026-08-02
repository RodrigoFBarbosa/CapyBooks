using System.Security.Claims;
using Asp.Versioning;
using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Common;
using CapyBooks.Application.DTOs.Reviews;
using CapyBooks.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapyBooks.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/books/{bookId:guid}/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<ReviewDto>>> GetByBook(
        Guid bookId, [FromQuery] ReviewSearchQueryDto query, CancellationToken cancellationToken)
        => Ok(await _reviewService.GetByBookAsync(bookId, query, cancellationToken));

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ReviewDto>> GetMyReview(Guid bookId, CancellationToken cancellationToken)
    {
        var review = await _reviewService.GetByUserAndBookAsync(CurrentUserId(), bookId, cancellationToken);
        return review is null ? NotFound() : Ok(review);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ReviewDto>> Create(Guid bookId, CreateReviewRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _reviewService.CreateAsync(bookId, CurrentUserId(), request, cancellationToken);
        return Created($"/api/v1/reviews/{result.Id}", result);
    }

    [Authorize]
    [HttpPut("~/api/v{version:apiVersion}/reviews/{id:guid}")]
    public async Task<ActionResult<ReviewDto>> Update(Guid id, UpdateReviewRequestDto request, CancellationToken cancellationToken)
        => Ok(await _reviewService.UpdateAsync(id, CurrentUserId(), request, cancellationToken));

    [Authorize]
    [HttpDelete("~/api/v{version:apiVersion}/reviews/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var isAdmin = User.IsInRole("Admin");
        await _reviewService.DeleteAsync(id, CurrentUserId(), isAdmin, cancellationToken);
        return NoContent();
    }

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
