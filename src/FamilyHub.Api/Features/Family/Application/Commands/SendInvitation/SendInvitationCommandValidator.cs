using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Family.Application.Commands.SendInvitation;

public class SendInvitationCommandValidator : AbstractValidator<SendInvitationCommand>, IInputValidator<SendInvitationCommand>
{
    public SendInvitationCommandValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        RuleFor(x => x.InviteeEmail.Value)
            .NotEmpty().WithMessage(_ => localizer["InviteeEmailRequired"])
            .EmailAddress().WithMessage(_ => localizer["InviteeEmailInvalidFormat"]);

        RuleFor(x => x.Role.Value)
            .Must(role => role is "Admin" or "Member")
            .WithMessage(_ => localizer["InvitationRoleInvalid"]);
    }
}
