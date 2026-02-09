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
public static class AcceptInvitationByIdCommandHandler
{
    public static async Task<AcceptInvitationResult> Handle(
        AcceptInvitationByIdCommand command,
        IFamilyInvitationRepository invitationRepository,
        IFamilyMemberRepository memberRepository,
        IUserRepository userRepository,
        CancellationToken ct)
    {
        var invitation = await invitationRepository.GetByIdAsync(command.InvitationId, ct)
            ?? throw new DomainException("Invitation not found");

        var user = await userRepository.GetByIdAsync(command.AcceptingUserId, ct)
            ?? throw new DomainException("User not found");

        // Security: verify the accepting user's email matches the invitation
        if (user.Email != invitation.InviteeEmail)
        {
            throw new DomainException("This invitation was sent to a different email address");
        }

        // Check if user is already a member of this family
        var existingMember = await memberRepository.GetByUserAndFamilyAsync(command.AcceptingUserId, invitation.FamilyId, ct);
        if (existingMember is not null)
        {
            throw new DomainException("You are already a member of this family");
        }

        // Accept the invitation (validates status + expiry, raises InvitationAcceptedEvent)
        invitation.Accept(command.AcceptingUserId);

        // Create FamilyMember record
        var member = FamilyMember.Create(invitation.FamilyId, command.AcceptingUserId, invitation.Role);
        await memberRepository.AddAsync(member, ct);

        // Assign user to family
        user.AssignToFamily(invitation.FamilyId);

        // Save all changes
        await invitationRepository.SaveChangesAsync(ct);

        return new AcceptInvitationResult(invitation.FamilyId, member.Id);
    }
}
