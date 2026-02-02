using FamilyHub.Api.Features.Auth.Application.Commands;
using FluentValidation;

namespace FamilyHub.Api.Features.Auth.Application.Validators;

/// <summary>
/// Validator for RegisterUserCommand.
/// Note: Vogen value objects already enforce basic validation,
/// this validator provides additional business rules.
/// </summary>
public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        // Vogen already validates non-empty, but we add extra business rules here
        RuleFor(x => x.Email)
            .NotNull()
            .WithMessage("Email is required");

        RuleFor(x => x.Name)
            .NotNull()
            .WithMessage("Name is required");

        RuleFor(x => x.ExternalUserId)
            .NotNull()
            .WithMessage("External user ID is required");

        // Optional username validation
        When(x => x.Username != null, () =>
        {
            RuleFor(x => x.Username)
                .MinimumLength(3)
                .WithMessage("Username must be at least 3 characters")
                .MaximumLength(50)
                .WithMessage("Username cannot exceed 50 characters")
                .Matches("^[a-zA-Z0-9_-]+$")
                .WithMessage("Username can only contain letters, numbers, underscores, and hyphens");
        });
    }
}
