using Asp.Versioning;
using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Genres;
using CapyBooks.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CapyBooks.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class GenresController : ControllerBase
{
    private readonly IGenreService _genreService;

    public GenresController(IGenreService genreService)
    {
        _genreService = genreService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GenreDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _genreService.GetAllAsync(cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<GenreDto>> Create(CreateGenreRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _genreService.CreateAsync(request, cancellationToken);
        return Created($"/api/v1/genres/{result.Id}", result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GenreDto>> Update(Guid id, UpdateGenreRequestDto request, CancellationToken cancellationToken)
        => Ok(await _genreService.UpdateAsync(id, request, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _genreService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
