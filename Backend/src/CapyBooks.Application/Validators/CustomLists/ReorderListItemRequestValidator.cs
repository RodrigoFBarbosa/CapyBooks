using CapyBooks.Application.DTOs.CustomLists;
using FluentValidation;

namespace CapyBooks.Application.Validators.CustomLists;

public class ReorderListItemRequestValidator : AbstractValidator<ReorderListItemRequestDto>
{
    public ReorderListItemRequestValidator()
    {
        RuleFor(x => x.NewIndex)
            .GreaterThanOrEqualTo(0);
    }
}
