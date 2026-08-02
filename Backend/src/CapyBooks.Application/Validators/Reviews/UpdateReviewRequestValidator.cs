using CapyBooks.Application.DTOs.Reviews;
using FluentValidation;

namespace CapyBooks.Application.Validators.Reviews;

public class UpdateReviewRequestValidator : AbstractValidator<UpdateReviewRequestDto>
{
    public UpdateReviewRequestValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.Comment)
            .MaximumLength(2000);
    }
}
