using FluentValidation;

namespace FamilyHub.Modules.Auth.Application.Commands.VerifyEmail;

/// <summary>
/// Validator for VerifyEmailCommand.
/// </summary>
public sealed class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VerifyEmailCommandValidator"/> class.
    /// </summary>
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Verification token is required.");
    }
}
