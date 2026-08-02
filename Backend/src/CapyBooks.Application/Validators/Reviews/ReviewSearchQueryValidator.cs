using CapyBooks.Application.DTOs.Reviews;
using FluentValidation;

namespace CapyBooks.Application.Validators.Reviews;

public class ReviewSearchQueryValidator : AbstractValidator<ReviewSearchQueryDto>
{
    public ReviewSearchQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
