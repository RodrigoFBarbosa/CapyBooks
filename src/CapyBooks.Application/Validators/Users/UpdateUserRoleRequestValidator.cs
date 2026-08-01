using CapyBooks.Application.DTOs.Users;
using CapyBooks.Domain.Enums;
using FluentValidation;

namespace CapyBooks.Application.Validators.Users;

public class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequestDto>
{
    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => Enum.TryParse<UserRole>(role, ignoreCase: true, out _))
            .WithMessage("Role inválida. Valores aceitos: User, Admin.");
    }
}
