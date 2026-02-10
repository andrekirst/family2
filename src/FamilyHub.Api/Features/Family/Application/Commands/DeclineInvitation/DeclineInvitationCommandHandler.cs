using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Common.Infrastructure.Security;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.DeclineInvitation;

/// <summary>
/// Handler for DeclineInvitationCommand.
/// Looks up invitation by token hash and declines it.
/// </summary>
public sealed class DeclineInvitationCommandHandler(
    IFamilyInvitationRepository invitationRepository)
    : ICommandHandler<DeclineInvitationCommand, bool>
{
    public async ValueTask<bool> Handle(
        DeclineInvitationCommand command,
        CancellationToken cancellationToken)
    {
        var tokenHash = SecureTokenHelper.ComputeSha256Hash(command.Token);
        var invitation = await invitationRepository.GetByTokenHashAsync(InvitationToken.From(tokenHash), cancellationToken)
            ?? throw new DomainException("Invalid invitation token");

        invitation.Decline();

        return true;
    }
}
