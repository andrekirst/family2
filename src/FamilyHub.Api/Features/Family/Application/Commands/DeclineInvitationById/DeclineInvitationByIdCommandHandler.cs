using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.Repositories;

namespace FamilyHub.Api.Features.Family.Application.Commands.DeclineInvitationById;

/// <summary>
/// Handler for DeclineInvitationByIdCommand.
/// Looks up invitation by ID, verifies email match, and declines it.
/// </summary>
public sealed class DeclineInvitationByIdCommandHandler(
    IFamilyInvitationRepository invitationRepository,
    IUserRepository userRepository)
    : ICommandHandler<DeclineInvitationByIdCommand, bool>
{
    public async ValueTask<bool> Handle(
        DeclineInvitationByIdCommand command,
        CancellationToken cancellationToken)
    {
        var invitation = await invitationRepository.GetByIdAsync(command.InvitationId, cancellationToken)
            ?? throw new DomainException("Invitation not found");

        var user = await userRepository.GetByIdAsync(command.DeclininingUserId, cancellationToken)
            ?? throw new DomainException("User not found");

        // Security: verify the declining user's email matches the invitation
        if (user.Email != invitation.InviteeEmail)
        {
            throw new DomainException("This invitation was sent to a different email address");
        }

        invitation.Decline();

        return true;
    }
}
