using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Application.Commands.SendInvitation;
using FamilyHub.Api.Features.Family.Application.Commands.Shared;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitation;

/// <summary>
/// Handler for AcceptInvitationCommand.
/// Looks up invitation by token hash, validates, creates FamilyMember, and assigns user.
/// </summary>
public static class AcceptInvitationCommandHandler
{
    public static async Task<AcceptInvitationResult> Handle(
        AcceptInvitationCommand command,
        IFamilyInvitationRepository invitationRepository,
        IFamilyMemberRepository memberRepository,
        IUserRepository userRepository,
        CancellationToken ct)
    {
        // Hash the plaintext token to look up the invitation
        var tokenHash = SendInvitationCommandHandler.ComputeSha256Hash(command.Token);
        var invitation = await invitationRepository.GetByTokenHashAsync(InvitationToken.From(tokenHash), ct)
            ?? throw new DomainException("Invalid invitation token");

        // Get the accepting user
        var user = await userRepository.GetByIdAsync(command.AcceptingUserId, ct)
            ?? throw new DomainException("User not found");

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
