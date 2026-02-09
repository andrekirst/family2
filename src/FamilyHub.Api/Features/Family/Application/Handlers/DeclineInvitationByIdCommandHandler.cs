using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Application.Commands;
using FamilyHub.Api.Features.Family.Domain.Repositories;

namespace FamilyHub.Api.Features.Family.Application.Handlers;

/// <summary>
/// Handler for DeclineInvitationByIdCommand.
/// Looks up invitation by ID, verifies email match, and declines it.
/// </summary>
public static class DeclineInvitationByIdCommandHandler
{
    public static async Task<bool> Handle(
        DeclineInvitationByIdCommand command,
        IFamilyInvitationRepository invitationRepository,
        IUserRepository userRepository,
        CancellationToken ct)
    {
        var invitation = await invitationRepository.GetByIdAsync(command.InvitationId, ct)
            ?? throw new DomainException("Invitation not found");

        var user = await userRepository.GetByIdAsync(command.DeclininingUserId, ct)
            ?? throw new DomainException("User not found");

        // Security: verify the declining user's email matches the invitation
        if (user.Email != invitation.InviteeEmail)
        {
            throw new DomainException("This invitation was sent to a different email address");
        }

        invitation.Decline();

        await invitationRepository.SaveChangesAsync(ct);

        return true;
    }
}
