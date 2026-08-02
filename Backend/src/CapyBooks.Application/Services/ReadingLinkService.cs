using CapyBooks.Application.Common.Exceptions;
using CapyBooks.Application.DTOs;
using CapyBooks.Application.DTOs.ReadingLinks;
using CapyBooks.Application.Interfaces;
using CapyBooks.Domain.Entities;
using CapyBooks.Domain.Interfaces;

namespace CapyBooks.Application.Services;

public class ReadingLinkService : IReadingLinkService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReadingLinkService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ReadingLinkDto>> GetByBookAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        var links = await _unitOfWork.ReadingLinks.GetByBookAsync(bookId, cancellationToken);
        return links.Select(ToDto).ToList();
    }

    public async Task<ReadingLinkDto> CreateAsync(Guid bookId, CreateReadingLinkRequestDto request, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Books.GetByIdAsync(bookId, cancellationToken) is null)
            throw new NotFoundException("Livro não encontrado.");

        var link = new ReadingLink(bookId, request.SourceName, request.Url);

        await _unitOfWork.ReadingLinks.AddAsync(link, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(link);
    }

    public async Task<ReadingLinkDto> UpdateAsync(Guid id, UpdateReadingLinkRequestDto request, CancellationToken cancellationToken = default)
    {
        var link = await _unitOfWork.ReadingLinks.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Link de leitura não encontrado.");

        link.Update(request.SourceName, request.Url);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(link);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var link = await _unitOfWork.ReadingLinks.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Link de leitura não encontrado.");

        _unitOfWork.ReadingLinks.Remove(link);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ReadingLinkDto ToDto(ReadingLink link) =>
        new(link.Id, link.BookId, link.SourceName, link.Url);
}
