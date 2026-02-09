using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Features.Family.Application.Services;
using FamilyHub.Api.Features.Family.Domain.Repositories;

namespace FamilyHub.Api.Features.Family.Application.Commands.RevokeInvitation;

/// <summary>
/// Handler for RevokeInvitationCommand.
/// Validates authorization and revokes a pending invitation.
/// </summary>
public static class RevokeInvitationCommandHandler
{
    public static async Task<bool> Handle(
        RevokeInvitationCommand command,
        FamilyAuthorizationService authService,
        IFamilyInvitationRepository invitationRepository,
        CancellationToken ct)
    {
        var invitation = await invitationRepository.GetByIdAsync(command.InvitationId, ct)
            ?? throw new DomainException("Invitation not found");

        // Authorization: only Owner/Admin of the family can revoke
        if (!await authService.CanInviteAsync(command.RevokedBy, invitation.FamilyId, ct))
        {
            throw new DomainException("You do not have permission to revoke invitations for this family");
        }

        invitation.Revoke();

        await invitationRepository.SaveChangesAsync(ct);

        return true;
    }
}
