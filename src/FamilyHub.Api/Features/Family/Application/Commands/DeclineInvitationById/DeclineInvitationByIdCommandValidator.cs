using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Family.Application.Commands.DeclineInvitationById;

public class DeclineInvitationByIdCommandValidator : AbstractValidator<DeclineInvitationByIdCommand>, IInputValidator<DeclineInvitationByIdCommand>
{
    public DeclineInvitationByIdCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.InvitationId.Value)
            .NotEmpty().WithMessage(_ => localizer["InvitationIdRequired"]);

        RuleFor(x => x.UserId.Value)
            .NotEmpty().WithMessage(_ => localizer["DecliningUserIdRequired"]);
    }
}
