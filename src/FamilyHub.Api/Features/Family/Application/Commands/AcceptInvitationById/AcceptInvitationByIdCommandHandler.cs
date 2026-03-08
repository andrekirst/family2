using FamilyHub.Api.Common.Infrastructure.Security;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Application.Commands.Shared;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;

namespace FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitationById;

/// <summary>
/// Handler for AcceptInvitationByIdCommand.
/// Looks up invitation by ID, verifies the accepting user's email matches the invitee email,
/// creates FamilyMember, and assigns user to family.
/// </summary>
public sealed class AcceptInvitationByIdCommandHandler(
    IFamilyInvitationRepository invitationRepository,
    IFamilyMemberRepository memberRepository,
    IUserRepository userRepository)
    : ICommandHandler<AcceptInvitationByIdCommand, AcceptInvitationResult>
{
    [SecurityCheck("IDOR")]
    public async ValueTask<AcceptInvitationResult> Handle(
        AcceptInvitationByIdCommand command,
        CancellationToken cancellationToken)
    {
        var invitation = (await invitationRepository.GetByIdAsync(command.InvitationId, cancellationToken))!;
        var user = (await userRepository.GetByIdAsync(command.UserId, cancellationToken))!;

        if (user.Email != invitation.InviteeEmail)
        {
            throw new DomainException("This invitation was sent to a different email address", DomainErrorCodes.InvitationEmailMismatch);
        }

        // Accept the invitation (validates status + expiry, raises InvitationAcceptedEvent)
        invitation.Accept(command.UserId);

        // Create FamilyMember record
        var member = FamilyMember.Create(invitation.FamilyId, command.UserId, invitation.Role);
        await memberRepository.AddAsync(member, cancellationToken);

        // Assign user to family
        user.AssignToFamily(invitation.FamilyId);

        return new AcceptInvitationResult(invitation.FamilyId, member.Id);
    }
}
