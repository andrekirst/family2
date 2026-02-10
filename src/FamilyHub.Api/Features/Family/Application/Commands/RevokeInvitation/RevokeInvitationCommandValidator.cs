using FluentValidation;

namespace FamilyHub.Api.Features.Family.Application.Commands.RevokeInvitation;

public class RevokeInvitationCommandValidator : AbstractValidator<RevokeInvitationCommand>
{
    public RevokeInvitationCommandValidator()
    {
        RuleFor(x => x.InvitationId.Value)
            .NotEmpty().WithMessage("Invitation ID is required");

        RuleFor(x => x.RevokedBy.Value)
            .NotEmpty().WithMessage("Revoking user ID is required");
    }
}
