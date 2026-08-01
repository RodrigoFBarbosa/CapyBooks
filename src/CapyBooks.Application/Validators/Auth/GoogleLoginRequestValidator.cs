using CapyBooks.Application.DTOs.Auth;
using FluentValidation;

namespace CapyBooks.Application.Validators.Auth;

public class GoogleLoginRequestValidator : AbstractValidator<GoogleLoginRequestDto>
{
    public GoogleLoginRequestValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty();
    }
}
