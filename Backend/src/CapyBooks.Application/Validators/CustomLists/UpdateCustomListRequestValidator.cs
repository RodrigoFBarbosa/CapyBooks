using CapyBooks.Application.DTOs.CustomLists;
using FluentValidation;

namespace CapyBooks.Application.Validators.CustomLists;

public class UpdateCustomListRequestValidator : AbstractValidator<UpdateCustomListRequestDto>
{
    public UpdateCustomListRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);
    }
}
