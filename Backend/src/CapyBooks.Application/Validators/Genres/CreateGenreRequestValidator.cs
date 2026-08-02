using CapyBooks.Application.DTOs.Genres;
using FluentValidation;

namespace CapyBooks.Application.Validators.Genres;

public class CreateGenreRequestValidator : AbstractValidator<CreateGenreRequestDto>
{
    public CreateGenreRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
