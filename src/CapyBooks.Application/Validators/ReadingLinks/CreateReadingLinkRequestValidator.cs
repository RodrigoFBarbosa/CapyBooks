using CapyBooks.Application.DTOs.ReadingLinks;
using FluentValidation;

namespace CapyBooks.Application.Validators.ReadingLinks;

public class CreateReadingLinkRequestValidator : AbstractValidator<CreateReadingLinkRequestDto>
{
    public CreateReadingLinkRequestValidator()
    {
        RuleFor(x => x.SourceName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Url)
            .NotEmpty()
            .MaximumLength(1000)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("A URL informada não é válida.");
    }
}
