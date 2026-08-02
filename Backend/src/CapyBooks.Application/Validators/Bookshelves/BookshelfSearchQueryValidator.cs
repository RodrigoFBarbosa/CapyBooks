using CapyBooks.Application.DTOs.Bookshelves;
using CapyBooks.Domain.Enums;
using FluentValidation;

namespace CapyBooks.Application.Validators.Bookshelves;

public class BookshelfSearchQueryValidator : AbstractValidator<BookshelfSearchQueryDto>
{
    public BookshelfSearchQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Status)
            .Must(status => Enum.TryParse<BookshelfStatus>(status, ignoreCase: true, out _))
            .WithMessage("Status inválido. Valores aceitos: WantToRead, Reading, Read.")
            .When(x => x.Status is not null);
    }
}
