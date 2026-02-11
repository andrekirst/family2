using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Common.Infrastructure.Security;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Application.Commands.Shared;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitation;

/// <summary>
/// Handler for AcceptInvitationCommand.
/// Looks up invitation by token hash, validates, creates FamilyMember, and assigns user.
/// </summary>
public sealed class AcceptInvitationCommandHandler(
    IFamilyInvitationRepository invitationRepository,
    IFamilyMemberRepository memberRepository,
    IUserRepository userRepository)
    : ICommandHandler<AcceptInvitationCommand, AcceptInvitationResult>
{
    public async ValueTask<AcceptInvitationResult> Handle(
        AcceptInvitationCommand command,
        CancellationToken cancellationToken)
    {
        // Hash the plaintext token to look up the invitation
        var tokenHash = SecureTokenHelper.ComputeSha256Hash(command.Token);
        var invitation = await invitationRepository.GetByTokenHashAsync(InvitationToken.From(tokenHash), cancellationToken)
            ?? throw new DomainException("Invalid invitation token");

        // Get the accepting user
        var user = await userRepository.GetByIdAsync(command.AcceptingUserId, cancellationToken)
            ?? throw new DomainException("User not found");

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
