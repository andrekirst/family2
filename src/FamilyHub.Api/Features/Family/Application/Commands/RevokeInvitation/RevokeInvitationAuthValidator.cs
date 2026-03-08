using FamilyHub.Api.Common.Infrastructure.Validation;
using FamilyHub.Api.Features.Family.Application.Services;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Resources;
using FamilyHub.Common.Domain;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace FamilyHub.Api.Features.Family.Application.Commands.RevokeInvitation;

/// <summary>
/// Authorization validator for RevokeInvitationCommand.
/// Checks that the invitation exists and that the revoking user has permission.
/// Combined existence + auth check because auth requires the entity to determine FamilyId.
/// </summary>
public sealed class RevokeInvitationAuthValidator : AbstractValidator<RevokeInvitationCommand>, IAuthValidator<RevokeInvitationCommand>
{
    public RevokeInvitationAuthValidator(
        IFamilyInvitationRepository invitationRepository,
        FamilyAuthorizationService authService,
        IStringLocalizer<DomainErrors> localizer)
    {
        RuleFor(x => x)
            .MustAsync(async (command, ct) =>
            {
                var invitation = await invitationRepository.GetByIdAsync(command.InvitationId, ct);
                if (invitation is null)
                {
                    return false;
                }

                return await authService.CanInviteAsync(command.UserId, invitation.FamilyId, ct);
            })
            .WithErrorCode(DomainErrorCodes.InsufficientPermissionToRevokeInvitation)
            .WithMessage(_ => localizer[DomainErrorCodes.InsufficientPermissionToRevokeInvitation].Value);
    }
}
