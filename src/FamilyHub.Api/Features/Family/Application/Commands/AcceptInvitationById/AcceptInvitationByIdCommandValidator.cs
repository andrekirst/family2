using FluentValidation;

namespace FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitationById;

public class AcceptInvitationByIdCommandValidator : AbstractValidator<AcceptInvitationByIdCommand>
{
    public AcceptInvitationByIdCommandValidator()
    {
        RuleFor(x => x.InvitationId.Value)
            .NotEmpty().WithMessage("Invitation ID is required");

        RuleFor(x => x.AcceptingUserId.Value)
            .NotEmpty().WithMessage("Accepting user ID is required");
    }
}
