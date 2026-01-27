using FluentValidation;

namespace FamilyHub.Modules.Auth.Application.Commands.ChangePassword;

/// <summary>
/// Validator for ChangePasswordCommand.
/// </summary>
public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordCommandValidator"/> class.
    /// </summary>
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required.")
            .MinimumLength(12)
            .WithMessage("New password must be at least 12 characters long.")
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("New password must be different from current password.");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty()
            .WithMessage("Password confirmation is required.")
            .Equal(x => x.NewPassword)
            .WithMessage("Passwords do not match.");
    }
}
