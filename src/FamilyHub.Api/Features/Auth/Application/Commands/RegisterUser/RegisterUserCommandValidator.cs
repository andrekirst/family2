using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Auth.Application.Commands.RegisterUser;

/// <summary>
/// Validator for RegisterUserCommand.
/// Note: Vogen value objects already enforce basic validation,
/// this validator provides additional business rules.
/// </summary>
public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        // Vogen already validates non-empty, but we add extra business rules here
        RuleFor(x => x.Email)
            .NotNull()
            .WithMessage(_ => localizer["EmailRequired"]);

        RuleFor(x => x.Name)
            .NotNull()
            .WithMessage(_ => localizer["NameRequired"]);

        RuleFor(x => x.ExternalUserId)
            .NotNull()
            .WithMessage(_ => localizer["ExternalUserIdRequired"]);

        // Optional username validation
        When(x => x.Username != null, () =>
        {
            RuleFor(x => x.Username)
                .MinimumLength(3)
                .WithMessage(_ => localizer["UsernameMinLength"])
                .MaximumLength(50)
                .WithMessage(_ => localizer["UsernameMaxLength"])
                .Matches("^[a-zA-Z0-9_-]+$")
                .WithMessage(_ => localizer["UsernameInvalidFormat"]);
        });
    }
}
