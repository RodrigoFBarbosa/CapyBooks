using CapyBooks.Application.DTOs.Users;
using FluentValidation;

namespace CapyBooks.Application.Validators.Users;

public class UserSearchQueryValidator : AbstractValidator<UserSearchQueryDto>
{
    public UserSearchQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
