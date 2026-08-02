using CapyBooks.Application.DTOs.CustomLists;
using FluentValidation;

namespace CapyBooks.Application.Validators.CustomLists;

public class CreateCustomListRequestValidator : AbstractValidator<CreateCustomListRequestDto>
{
    public CreateCustomListRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);
    }
}
