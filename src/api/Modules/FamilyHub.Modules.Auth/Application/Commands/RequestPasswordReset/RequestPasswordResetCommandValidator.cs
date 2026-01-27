using FluentValidation;

namespace FamilyHub.Modules.Auth.Application.Commands.RequestPasswordReset;

/// <summary>
/// Validator for RequestPasswordResetCommand.
/// </summary>
public sealed class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequestPasswordResetCommandValidator"/> class.
    /// </summary>
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.");
    }
}
