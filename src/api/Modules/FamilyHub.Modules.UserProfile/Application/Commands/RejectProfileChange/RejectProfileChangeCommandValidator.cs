using FluentValidation;

namespace FamilyHub.Modules.UserProfile.Application.Commands.RejectProfileChange;

/// <summary>
/// Validator for RejectProfileChangeCommand.
/// </summary>
public sealed class RejectProfileChangeCommandValidator : AbstractValidator<RejectProfileChangeCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RejectProfileChangeCommandValidator"/> class.
    /// </summary>
    public RejectProfileChangeCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty()
            .WithMessage("Request ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Rejection reason is required.")
            .MinimumLength(10)
            .WithMessage("Rejection reason must be at least 10 characters to provide meaningful feedback.");
    }
}
