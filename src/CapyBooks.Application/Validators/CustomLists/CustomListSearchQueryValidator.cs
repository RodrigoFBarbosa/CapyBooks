using CapyBooks.Application.DTOs.CustomLists;
using FluentValidation;

namespace CapyBooks.Application.Validators.CustomLists;

public class CustomListSearchQueryValidator : AbstractValidator<CustomListSearchQueryDto>
{
    public CustomListSearchQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
