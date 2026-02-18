using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitationById;

public class AcceptInvitationByIdCommandValidator : AbstractValidator<AcceptInvitationByIdCommand>
{
    public AcceptInvitationByIdCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.InvitationId.Value)
            .NotEmpty().WithMessage(_ => localizer["InvitationIdRequired"]);

        RuleFor(x => x.AcceptingUserId.Value)
            .NotEmpty().WithMessage(_ => localizer["AcceptingUserIdRequired"]);
    }
}
