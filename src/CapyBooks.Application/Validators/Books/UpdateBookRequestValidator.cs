using CapyBooks.Application.DTOs.Books;
using FluentValidation;

namespace CapyBooks.Application.Validators.Books;

public class UpdateBookRequestValidator : AbstractValidator<UpdateBookRequestDto>
{
    public UpdateBookRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Author)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.Isbn)
            .MaximumLength(20);

        RuleFor(x => x.CoverUrl)
            .MaximumLength(1000);

        RuleFor(x => x.PublishedYear)
            .InclusiveBetween(0, DateTime.UtcNow.Year + 1)
            .When(x => x.PublishedYear.HasValue);

        RuleForEach(x => x.GenreIds)
            .NotEmpty();
    }
}
