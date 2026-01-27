using FluentValidation;

namespace FamilyHub.Modules.Auth.Application.Commands.ResetPasswordWithCode;

/// <summary>
/// Validator for ResetPasswordWithCodeCommand.
/// </summary>
public sealed class ResetPasswordWithCodeCommandValidator : AbstractValidator<ResetPasswordWithCodeCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResetPasswordWithCodeCommandValidator"/> class.
    /// </summary>
    public ResetPasswordWithCodeCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Reset code is required.")
            .Length(6)
            .WithMessage("Reset code must be exactly 6 digits.")
            .Matches("^[0-9]+$")
            .WithMessage("Reset code must contain only digits.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required.")
            .MinimumLength(12)
            .WithMessage("New password must be at least 12 characters long.");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty()
            .WithMessage("Password confirmation is required.")
            .Equal(x => x.NewPassword)
            .WithMessage("Passwords do not match.");
    }
}
