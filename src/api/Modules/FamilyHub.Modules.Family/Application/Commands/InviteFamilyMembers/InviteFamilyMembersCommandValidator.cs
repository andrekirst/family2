using FluentValidation;

namespace FamilyHub.Modules.Family.Application.Commands.InviteFamilyMembers;

/// <summary>
/// Validator for InviteFamilyMembersCommand.
/// Validates command-level constraints only (max count, non-empty list).
/// Individual invitation validation happens in the handler to support partial success.
/// </summary>
public sealed class InviteFamilyMembersCommandValidator : AbstractValidator<InviteFamilyMembersCommand>
{
    /// <summary>
    /// Maximum number of invitations allowed in a single batch operation.
    /// </summary>
    public const int MaxInvitationsPerBatch = 20;

    /// <summary>
    /// Initializes a new instance of the <see cref="InviteFamilyMembersCommandValidator"/> class.
    /// </summary>
    public InviteFamilyMembersCommandValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty()
            .WithMessage("Family ID is required.");

        RuleFor(x => x.Invitations)
            .NotNull()
            .WithMessage("Invitations list is required.")
            .NotEmpty()
            .WithMessage("At least one invitation is required.")
            .Must(invitations => invitations.Count <= MaxInvitationsPerBatch)
            .WithMessage($"Cannot process more than {MaxInvitationsPerBatch} invitations per batch.");

        RuleFor(x => x.Message)
            .MaximumLength(1000)
            .WithMessage("Message cannot exceed 1000 characters.");
    }
}
