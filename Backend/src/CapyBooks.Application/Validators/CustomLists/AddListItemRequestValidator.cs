using CapyBooks.Application.DTOs.CustomLists;
using FluentValidation;

namespace CapyBooks.Application.Validators.CustomLists;

public class AddListItemRequestValidator : AbstractValidator<AddListItemRequestDto>
{
    public AddListItemRequestValidator()
    {
        RuleFor(x => x.BookId)
            .NotEmpty();
    }
}
