using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitation;

public class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>, IInputValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(_ => localizer["InvitationTokenRequired"]);

        RuleFor(x => x.UserId.Value)
            .NotEmpty().WithMessage(_ => localizer["AcceptingUserIdRequired"]);
    }
}
