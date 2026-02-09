using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Features.Family.Application.Commands;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Handlers;

/// <summary>
/// Handler for DeclineInvitationCommand.
/// Looks up invitation by token hash and declines it.
/// </summary>
public static class DeclineInvitationCommandHandler
{
    public static async Task<bool> Handle(
        DeclineInvitationCommand command,
        IFamilyInvitationRepository invitationRepository,
        CancellationToken ct)
    {
        var tokenHash = SendInvitationCommandHandler.ComputeSha256Hash(command.Token);
        var invitation = await invitationRepository.GetByTokenHashAsync(InvitationToken.From(tokenHash), ct)
            ?? throw new DomainException("Invalid invitation token");

        invitation.Decline();

        await invitationRepository.SaveChangesAsync(ct);

        return true;
    }
}
