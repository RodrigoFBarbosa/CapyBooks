using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Genres;

namespace CapyBooks.Application.Interfaces;

public interface IGenreService
{
    Task<IReadOnlyList<GenreDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<GenreDto> CreateAsync(CreateGenreRequestDto request, CancellationToken cancellationToken = default);

    Task<GenreDto> UpdateAsync(Guid id, UpdateGenreRequestDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
