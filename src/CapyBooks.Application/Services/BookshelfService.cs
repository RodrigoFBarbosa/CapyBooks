using CapyBooks.Application.Common.Exceptions;
using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Bookshelves;
using CapyBooks.Application.DTOs.Common;
using CapyBooks.Application.Interfaces;
using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Enums;
using CapyBooks.Domain.Interfaces;

namespace CapyBooks.Application.Services;

public class BookshelfService : IBookshelfService
{
    private readonly IUnitOfWork _unitOfWork;

    public BookshelfService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResultDto<BookshelfDto>> GetByUserAsync(
        Guid userId, BookshelfSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        BookshelfStatus? status = query.Status is null
            ? null
            : Enum.Parse<BookshelfStatus>(query.Status, ignoreCase: true);

        var (items, totalCount) = await _unitOfWork.Bookshelves.GetByUserAsync(
            userId, status, query.Page, query.PageSize, cancellationToken);

        var dtos = await MapWithBooksAsync(items, cancellationToken);

        return new PagedResultDto<BookshelfDto>(dtos, query.Page, query.PageSize, totalCount);
    }

    public async Task<BookshelfDto?> GetByUserAndBookAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default)
    {
        var entry = await _unitOfWork.Bookshelves.GetByUserAndBookAsync(userId, bookId, cancellationToken);
        if (entry is null)
            return null;

        var dtos = await MapWithBooksAsync([entry], cancellationToken);
        return dtos[0];
    }

    public async Task<BookshelfDto> SetStatusAsync(
        Guid userId, Guid bookId, SetBookshelfStatusRequestDto request, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Books.GetByIdAsync(bookId, cancellationToken) is null)
            throw new NotFoundException("Livro não encontrado.");

        var status = Enum.Parse<BookshelfStatus>(request.Status, ignoreCase: true);
        var entry = await _unitOfWork.Bookshelves.GetByUserAndBookAsync(userId, bookId, cancellationToken);

        if (entry is null)
        {
            entry = new Bookshelf(userId, bookId, status);
            await _unitOfWork.Bookshelves.AddAsync(entry, cancellationToken);
        }
        else
        {
            entry.UpdateStatus(status);
            _unitOfWork.Bookshelves.Update(entry);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dtos = await MapWithBooksAsync([entry], cancellationToken);
        return dtos[0];
    }

    public async Task RemoveAsync(Guid userId, Guid bookId, CancellationToken cancellationToken = default)
    {
        var entry = await _unitOfWork.Bookshelves.GetByUserAndBookAsync(userId, bookId, cancellationToken)
            ?? throw new NotFoundException("Este livro não está na sua estante.");

        _unitOfWork.Bookshelves.Remove(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<BookshelfDto>> MapWithBooksAsync(IReadOnlyList<Bookshelf> entries, CancellationToken cancellationToken)
    {
        var bookIds = entries.Select(e => e.BookId).Distinct().ToList();
        var books = await _unitOfWork.Books.GetByIdsAsync(bookIds, cancellationToken);
        var booksById = books.ToDictionary(b => b.Id);

        return entries
            .Select(e =>
            {
                booksById.TryGetValue(e.BookId, out var book);

                return new BookshelfDto(
                    e.Id,
                    e.BookId,
                    book?.Title ?? "Livro removido",
                    book?.Author ?? string.Empty,
                    book?.CoverUrl,
                    e.Status.ToString());
            })
            .ToList();
    }
}
