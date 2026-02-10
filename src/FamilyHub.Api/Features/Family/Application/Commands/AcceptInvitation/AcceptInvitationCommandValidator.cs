using FluentValidation;

namespace FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitation;

public class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Invitation token is required");

        RuleFor(x => x.AcceptingUserId.Value)
            .NotEmpty().WithMessage("Accepting user ID is required");
    }
}
