using CapyBooks.Application.DTOs.Genres;
using FluentValidation;

namespace CapyBooks.Application.Validators.Genres;

public class UpdateGenreRequestValidator : AbstractValidator<UpdateGenreRequestDto>
{
    public UpdateGenreRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
