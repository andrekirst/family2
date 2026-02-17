using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Family.Application.Commands.RevokeInvitation;

public class RevokeInvitationCommandValidator : AbstractValidator<RevokeInvitationCommand>
{
    public RevokeInvitationCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.InvitationId.Value)
            .NotEmpty().WithMessage(_ => localizer["InvitationIdRequired"]);

        RuleFor(x => x.RevokedBy.Value)
            .NotEmpty().WithMessage(_ => localizer["RevokingUserIdRequired"]);
    }
}
