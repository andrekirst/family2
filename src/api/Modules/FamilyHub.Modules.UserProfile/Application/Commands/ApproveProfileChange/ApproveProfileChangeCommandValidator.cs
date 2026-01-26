using FluentValidation;

namespace FamilyHub.Modules.UserProfile.Application.Commands.ApproveProfileChange;

/// <summary>
/// Validator for ApproveProfileChangeCommand.
/// </summary>
public sealed class ApproveProfileChangeCommandValidator : AbstractValidator<ApproveProfileChangeCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApproveProfileChangeCommandValidator"/> class.
    /// </summary>
    public ApproveProfileChangeCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty()
            .WithMessage("Request ID is required.");
    }
}
