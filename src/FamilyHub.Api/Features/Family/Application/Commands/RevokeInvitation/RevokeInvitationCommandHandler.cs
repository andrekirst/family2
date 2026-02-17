using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.Family.Application.Services;
using FamilyHub.Api.Features.Family.Domain.Repositories;

namespace FamilyHub.Api.Features.Family.Application.Commands.RevokeInvitation;

/// <summary>
/// Handler for RevokeInvitationCommand.
/// Validates authorization and revokes a pending invitation.
/// </summary>
public sealed class RevokeInvitationCommandHandler(
    FamilyAuthorizationService authService,
    IFamilyInvitationRepository invitationRepository)
    : ICommandHandler<RevokeInvitationCommand, bool>
{
    public async ValueTask<bool> Handle(
        RevokeInvitationCommand command,
        CancellationToken cancellationToken)
    {
        var invitation = await invitationRepository.GetByIdAsync(command.InvitationId, cancellationToken)
            ?? throw new DomainException("Invitation not found", DomainErrorCodes.InvitationNotFound);

        // Authorization: only Owner/Admin of the family can revoke
        if (!await authService.CanInviteAsync(command.RevokedBy, invitation.FamilyId, cancellationToken))
        {
            throw new DomainException("You do not have permission to revoke invitations for this family", DomainErrorCodes.InsufficientPermissionToRevokeInvitation);
        }

        invitation.Revoke();

        return true;
    }
}
