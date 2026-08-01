using CapyBooks.Application.DTOs.Bookshelves;
using CapyBooks.Domain.Enums;
using FluentValidation;

namespace CapyBooks.Application.Validators.Bookshelves;

public class SetBookshelfStatusRequestValidator : AbstractValidator<SetBookshelfStatusRequestDto>
{
    public SetBookshelfStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(status => Enum.TryParse<BookshelfStatus>(status, ignoreCase: true, out _))
            .WithMessage("Status inválido. Valores aceitos: WantToRead, Reading, Read.");
    }
}
