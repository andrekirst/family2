using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitation;

public class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(_ => localizer["InvitationTokenRequired"]);

        RuleFor(x => x.AcceptingUserId.Value)
            .NotEmpty().WithMessage(_ => localizer["AcceptingUserIdRequired"]);
    }
}
