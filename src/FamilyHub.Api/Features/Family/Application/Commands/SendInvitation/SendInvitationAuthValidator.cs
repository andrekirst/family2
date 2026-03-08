using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Family.Application.Services;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Family.Application.Commands.SendInvitation;

/// <summary>
/// Authorization validator for SendInvitationCommand.
/// Checks that the inviting user has permission to send invitations for the family.
/// </summary>
public sealed class SendInvitationAuthValidator : AbstractValidator<SendInvitationCommand>, IAuthValidator<SendInvitationCommand>
{
    public SendInvitationAuthValidator(
        FamilyAuthorizationService authService,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
            {
                return await authService.CanInviteAsync(command.InvitedBy, command.FamilyId, ct);
            })
            .WithErrorCode(DomainErrorCodes.InsufficientPermissionToSendInvitation)
            .WithMessage(_ => localizer[DomainErrorCodes.InsufficientPermissionToSendInvitation].Value);
    }
}
