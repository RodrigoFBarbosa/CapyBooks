using CapyBooks.Application.Common.Exceptions;
using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.Common;
using CapyBooks.Application.DTOs.CustomLists;
using CapyBooks.Application.Interfaces;
using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Interfaces;

namespace CapyBooks.Application.Services;

public class CustomListService : ICustomListService
{
    private readonly IUnitOfWork _unitOfWork;

    public CustomListService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResultDto<CustomListDto>> SearchAsync(CustomListSearchQueryDto query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.CustomLists.SearchAsync(query.UserId, query.Page, query.PageSize, cancellationToken);

        var userIds = items.Select(l => l.UserId).Distinct().ToList();
        var users = await _unitOfWork.Users.GetByIdsAsync(userIds, cancellationToken);
        var userNames = users.ToDictionary(u => u.Id, u => u.Name);

        var dtos = items
            .Select(l => new CustomListDto(
                l.Id,
                l.UserId,
                userNames.TryGetValue(l.UserId, out var name) ? name : "Usuário removido",
                l.Name,
                l.Description,
                l.Items.Count))
            .ToList();

        return new PagedResultDto<CustomListDto>(dtos, query.Page, query.PageSize, totalCount);
    }

    public async Task<CustomListDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var list = await _unitOfWork.CustomLists.GetByIdAsync(id, cancellationToken);
        return list is null ? null : await MapDetailAsync(list, cancellationToken);
    }

    public async Task<CustomListDetailDto> CreateAsync(Guid userId, CreateCustomListRequestDto request, CancellationToken cancellationToken = default)
    {
        var list = new CustomList(userId, request.Name, request.Description);

        await _unitOfWork.CustomLists.AddAsync(list, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapDetailAsync(list, cancellationToken);
    }

    public async Task<CustomListDetailDto> UpdateAsync(Guid id, Guid userId, UpdateCustomListRequestDto request, CancellationToken cancellationToken = default)
    {
        var list = await GetOwnedListAsync(id, userId, cancellationToken);

        list.Rename(request.Name);
        list.UpdateDescription(request.Description);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapDetailAsync(list, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var list = await GetOwnedListAsync(id, userId, cancellationToken);

        _unitOfWork.CustomLists.Remove(list);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CustomListDetailDto> AddItemAsync(Guid id, Guid userId, AddListItemRequestDto request, CancellationToken cancellationToken = default)
    {
        var list = await GetOwnedListAsync(id, userId, cancellationToken);

        if (await _unitOfWork.Books.GetByIdAsync(request.BookId, cancellationToken) is null)
            throw new NotFoundException("Livro não encontrado.");

        list.AddBook(request.BookId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapDetailAsync(list, cancellationToken);
    }

    public async Task<CustomListDetailDto> RemoveItemAsync(Guid id, Guid userId, Guid bookId, CancellationToken cancellationToken = default)
    {
        var list = await GetOwnedListAsync(id, userId, cancellationToken);

        list.RemoveBook(bookId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapDetailAsync(list, cancellationToken);
    }

    public async Task<CustomListDetailDto> ReorderItemAsync(
        Guid id, Guid userId, Guid bookId, ReorderListItemRequestDto request, CancellationToken cancellationToken = default)
    {
        var list = await GetOwnedListAsync(id, userId, cancellationToken);

        list.ReorderBook(bookId, request.NewIndex);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapDetailAsync(list, cancellationToken);
    }

    private async Task<CustomList> GetOwnedListAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var list = await _unitOfWork.CustomLists.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Lista não encontrada.");

        if (list.UserId != userId)
            throw new ForbiddenException("Você só pode gerenciar suas próprias listas.");

        return list;
    }

    private async Task<CustomListDetailDto> MapDetailAsync(CustomList list, CancellationToken cancellationToken)
    {
        var owner = await _unitOfWork.Users.GetByIdAsync(list.UserId, cancellationToken);

        var bookIds = list.Items.Select(i => i.BookId).ToList();
        var books = await _unitOfWork.Books.GetByIdsAsync(bookIds, cancellationToken);
        var booksById = books.ToDictionary(b => b.Id);

        var items = list.Items
            .OrderBy(i => i.Order)
            .Select(i =>
            {
                booksById.TryGetValue(i.BookId, out var book);
                return new CustomListItemDto(i.BookId, book?.Title ?? "Livro removido", book?.Author ?? string.Empty, book?.CoverUrl, i.Order);
            })
            .ToList();

        return new CustomListDetailDto(list.Id, list.UserId, owner?.Name ?? "Usuário removido", list.Name, list.Description, items);
    }
}
