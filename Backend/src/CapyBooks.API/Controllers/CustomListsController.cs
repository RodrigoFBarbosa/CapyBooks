using System.Security.Claims;
using Asp.Versioning;
using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Common;
using CapyBooks.Application.DTOs.CustomLists;
using CapyBooks.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapyBooks.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/lists")]
public class CustomListsController : ControllerBase
{
    private readonly ICustomListService _customListService;

    public CustomListsController(ICustomListService customListService)
    {
        _customListService = customListService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<CustomListDto>>> Search(
        [FromQuery] CustomListSearchQueryDto query, CancellationToken cancellationToken)
        => Ok(await _customListService.SearchAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomListDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var list = await _customListService.GetByIdAsync(id, cancellationToken);
        return list is null ? NotFound() : Ok(list);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CustomListDetailDto>> Create(CreateCustomListRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _customListService.CreateAsync(CurrentUserId(), request, cancellationToken);
        return Created($"/api/v1/lists/{result.Id}", result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomListDetailDto>> Update(Guid id, UpdateCustomListRequestDto request, CancellationToken cancellationToken)
        => Ok(await _customListService.UpdateAsync(id, CurrentUserId(), request, cancellationToken));

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _customListService.DeleteAsync(id, CurrentUserId(), cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id:guid}/items")]
    public async Task<ActionResult<CustomListDetailDto>> AddItem(Guid id, AddListItemRequestDto request, CancellationToken cancellationToken)
        => Ok(await _customListService.AddItemAsync(id, CurrentUserId(), request, cancellationToken));

    [Authorize]
    [HttpDelete("{id:guid}/items/{bookId:guid}")]
    public async Task<ActionResult<CustomListDetailDto>> RemoveItem(Guid id, Guid bookId, CancellationToken cancellationToken)
        => Ok(await _customListService.RemoveItemAsync(id, CurrentUserId(), bookId, cancellationToken));

    [Authorize]
    [HttpPut("{id:guid}/items/{bookId:guid}/order")]
    public async Task<ActionResult<CustomListDetailDto>> ReorderItem(
        Guid id, Guid bookId, ReorderListItemRequestDto request, CancellationToken cancellationToken)
        => Ok(await _customListService.ReorderItemAsync(id, CurrentUserId(), bookId, request, cancellationToken));

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
