using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Family.Application.Commands.DeclineInvitation;

public class DeclineInvitationCommandValidator : AbstractValidator<DeclineInvitationCommand>
{
    public DeclineInvitationCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(_ => localizer["InvitationTokenRequired"]);
    }
}
