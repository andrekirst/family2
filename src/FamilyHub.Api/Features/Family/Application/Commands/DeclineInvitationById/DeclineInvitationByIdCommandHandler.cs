using FamilyHub.Api.Common.Infrastructure.Security;
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
    [SecurityCheck("IDOR")]
    public async ValueTask<bool> Handle(
        DeclineInvitationByIdCommand command,
        CancellationToken cancellationToken)
    {
        var invitation = (await invitationRepository.GetByIdAsync(command.InvitationId, cancellationToken))!;
        var user = (await userRepository.GetByIdAsync(command.DeclininingUserId, cancellationToken))!;

        if (user.Email != invitation.InviteeEmail)
        {
            throw new DomainException("This invitation was sent to a different email address", DomainErrorCodes.InvitationEmailMismatch);
        }

        invitation.Decline();

        return true;
    }
}
