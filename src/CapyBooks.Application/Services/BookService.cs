using AutoMapper;
using CapyBooks.Application.Common.Exceptions;
using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Books;
using CapyBooks.Application.DTOs.Common;
using CapyBooks.Application.Interfaces;
using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Interfaces;

namespace CapyBooks.Application.Services;

public class BookService : IBookService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExternalBookSearchService _externalBookSearchService;
    private readonly IMapper _mapper;

    public BookService(IUnitOfWork unitOfWork, IExternalBookSearchService externalBookSearchService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _externalBookSearchService = externalBookSearchService;
        _mapper = mapper;
    }

    public async Task<BookDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var book = await _unitOfWork.Books.GetByIdAsync(id, cancellationToken);
        return book is null ? null : _mapper.Map<BookDto>(book);
    }

    public async Task<PagedResultDto<BookDto>> SearchAsync(BookSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Books.SearchAsync(
            query.Search, query.GenreId, query.Page, query.PageSize, cancellationToken);

        var dtos = items.Select(b => _mapper.Map<BookDto>(b)).ToList();

        return new PagedResultDto<BookDto>(dtos, query.Page, query.PageSize, totalCount);
    }

    public async Task<IReadOnlyList<ExternalBookResultDto>> SearchExternalAsync(
        ExternalBookSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(query.Isbn))
        {
            var result = await _externalBookSearchService.SearchByIsbnAsync(query.Isbn, cancellationToken);
            return result is null ? [] : [result];
        }

        return await _externalBookSearchService.SearchByTitleAsync(query.Title!, cancellationToken);
    }

    public async Task<BookDto> CreateAsync(CreateBookRequestDto request, Guid adminId, CancellationToken cancellationToken = default)
    {
        var book = new Book(
            request.Title,
            request.Author,
            adminId,
            request.Isbn,
            request.Synopsis,
            request.CoverUrl,
            request.PublishedYear,
            request.OpenLibraryId,
            request.GoogleBooksId);

        if (request.GenreIds.Count > 0)
        {
            var genres = await _unitOfWork.Genres.GetByIdsAsync(request.GenreIds, cancellationToken);
            foreach (var genre in genres)
                book.AddGenre(genre);
        }

        await _unitOfWork.Books.AddAsync(book, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<BookDto>(book);
    }

    public async Task<BookDto> UpdateAsync(Guid id, UpdateBookRequestDto request, CancellationToken cancellationToken = default)
    {
        var book = await _unitOfWork.Books.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Livro não encontrado.");

        book.Update(request.Title, request.Author, request.Isbn, request.Synopsis, request.CoverUrl, request.PublishedYear);

        await SyncGenresAsync(book, request.GenreIds, cancellationToken);

        _unitOfWork.Books.Update(book);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<BookDto>(book);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var book = await _unitOfWork.Books.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Livro não encontrado.");

        _unitOfWork.Books.Remove(book);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task SyncGenresAsync(Book book, IReadOnlyList<Guid> genreIds, CancellationToken cancellationToken)
    {
        var currentGenreIds = book.Genres.Select(g => g.Id).ToHashSet();
        var newGenreIds = genreIds.ToHashSet();

        foreach (var genre in book.Genres.Where(g => !newGenreIds.Contains(g.Id)).ToList())
            book.RemoveGenre(genre);

        var genreIdsToAdd = newGenreIds.Except(currentGenreIds).ToList();
        if (genreIdsToAdd.Count == 0)
            return;

        var genresToAdd = await _unitOfWork.Genres.GetByIdsAsync(genreIdsToAdd, cancellationToken);
        foreach (var genre in genresToAdd)
            book.AddGenre(genre);
    }
}
