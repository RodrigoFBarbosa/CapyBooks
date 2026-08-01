using CapyBooks.Application.DTOs.Books;
using FluentValidation;

namespace CapyBooks.Application.Validators.Books;

public class BookSearchQueryValidator : AbstractValidator<BookSearchQueryDto>
{
    public BookSearchQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
