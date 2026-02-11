using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Common.Domain;
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
    public async ValueTask<AcceptInvitationResult> Handle(
        AcceptInvitationByIdCommand command,
        CancellationToken cancellationToken)
    {
        var invitation = await invitationRepository.GetByIdAsync(command.InvitationId, cancellationToken)
            ?? throw new DomainException("Invitation not found");

        var user = await userRepository.GetByIdAsync(command.AcceptingUserId, cancellationToken)
            ?? throw new DomainException("User not found");

        // Security: verify the accepting user's email matches the invitation
        if (user.Email != invitation.InviteeEmail)
        {
            throw new DomainException("This invitation was sent to a different email address");
        }

        // Check if user is already a member of this family
        var existingMember = await memberRepository.GetByUserAndFamilyAsync(command.AcceptingUserId, invitation.FamilyId, cancellationToken);
        if (existingMember is not null)
        {
            throw new DomainException("You are already a member of this family");
        }

        // Accept the invitation (validates status + expiry, raises InvitationAcceptedEvent)
        invitation.Accept(command.AcceptingUserId);

        // Create FamilyMember record
        var member = FamilyMember.Create(invitation.FamilyId, command.AcceptingUserId, invitation.Role);
        await memberRepository.AddAsync(member, cancellationToken);

        // Assign user to family
        user.AssignToFamily(invitation.FamilyId);

        return new AcceptInvitationResult(invitation.FamilyId, member.Id);
    }
}
