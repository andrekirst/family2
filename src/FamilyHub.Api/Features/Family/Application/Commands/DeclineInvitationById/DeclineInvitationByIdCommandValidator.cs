using FluentValidation;

namespace FamilyHub.Api.Features.Family.Application.Commands.DeclineInvitationById;

public class DeclineInvitationByIdCommandValidator : AbstractValidator<DeclineInvitationByIdCommand>
{
    public DeclineInvitationByIdCommandValidator()
    {
        RuleFor(x => x.InvitationId.Value)
            .NotEmpty().WithMessage("Invitation ID is required");

        RuleFor(x => x.DeclininingUserId.Value)
            .NotEmpty().WithMessage("Declining user ID is required");
    }
}
