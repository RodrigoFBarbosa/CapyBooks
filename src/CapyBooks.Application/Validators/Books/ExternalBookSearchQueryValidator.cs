using CapyBooks.Application.DTOs.Books;
using FluentValidation;

namespace CapyBooks.Application.Validators.Books;

public class ExternalBookSearchQueryValidator : AbstractValidator<ExternalBookSearchQueryDto>
{
    public ExternalBookSearchQueryValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Title) || !string.IsNullOrWhiteSpace(x.Isbn))
            .WithMessage("Informe um título ou ISBN para buscar.");
    }
}
