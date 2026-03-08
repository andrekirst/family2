using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Family.Domain.Repositories;

namespace FamilyHub.Api.Features.Family.Application.Commands.RevokeInvitation;

/// <summary>
/// Handler for RevokeInvitationCommand.
/// Validates authorization and revokes a pending invitation.
/// </summary>
public sealed class RevokeInvitationCommandHandler(
    IFamilyInvitationRepository invitationRepository)
    : ICommandHandler<RevokeInvitationCommand, bool>
{
    public async ValueTask<bool> Handle(
        RevokeInvitationCommand command,
        CancellationToken cancellationToken)
    {
        // Fetch invitation (auth validator guarantees existence and permission)
        var invitation = (await invitationRepository.GetByIdAsync(command.InvitationId!.Value, cancellationToken))!;

        invitation.Revoke();

        return true;
    }
}
