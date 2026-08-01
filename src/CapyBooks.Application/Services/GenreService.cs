using AutoMapper;
using CapyBooks.Application.Common.Exceptions;
using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Genres;
using CapyBooks.Application.Interfaces;
using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Interfaces;

namespace CapyBooks.Application.Services;

public class GenreService : IGenreService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GenreService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<GenreDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var genres = await _unitOfWork.Genres.GetAllAsync(cancellationToken);
        return genres.Select(g => _mapper.Map<GenreDto>(g)).ToList();
    }

    public async Task<GenreDto> CreateAsync(CreateGenreRequestDto request, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Genres.GetByNameAsync(request.Name, cancellationToken) is not null)
            throw new ConflictException("Já existe um gênero com esse nome.");

        var genre = new Genre(request.Name);

        await _unitOfWork.Genres.AddAsync(genre, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<GenreDto>(genre);
    }

    public async Task<GenreDto> UpdateAsync(Guid id, UpdateGenreRequestDto request, CancellationToken cancellationToken = default)
    {
        var genre = await _unitOfWork.Genres.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Gênero não encontrado.");

        var existing = await _unitOfWork.Genres.GetByNameAsync(request.Name, cancellationToken);
        if (existing is not null && existing.Id != id)
            throw new ConflictException("Já existe um gênero com esse nome.");

        genre.Rename(request.Name);

        _unitOfWork.Genres.Update(genre);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<GenreDto>(genre);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var genre = await _unitOfWork.Genres.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Gênero não encontrado.");

        _unitOfWork.Genres.Remove(genre);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
