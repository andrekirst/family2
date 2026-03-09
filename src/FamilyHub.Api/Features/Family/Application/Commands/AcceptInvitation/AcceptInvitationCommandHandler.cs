using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.Security;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Application.Commands.Shared;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.AcceptInvitation;

/// <summary>
/// Handler for AcceptInvitationCommand.
/// Looks up invitation by token hash, validates, creates FamilyMember, and assigns user.
/// </summary>
public sealed class AcceptInvitationCommandHandler(
    IFamilyInvitationRepository invitationRepository,
    IFamilyMemberRepository memberRepository,
    IUserRepository userRepository,
    TimeProvider timeProvider)
    : ICommandHandler<AcceptInvitationCommand, AcceptInvitationResult>
{
    public async ValueTask<AcceptInvitationResult> Handle(
        AcceptInvitationCommand command,
        CancellationToken cancellationToken)
    {
        // Hash the plaintext token to look up the invitation (validator guarantees existence)
        var tokenHash = SecureTokenHelper.ComputeSha256Hash(command.Token);
        var invitation = (await invitationRepository.GetByTokenHashAsync(InvitationToken.From(tokenHash), cancellationToken))!;

        // Get the accepting user (validator guarantees existence)
        var user = (await userRepository.GetByIdAsync(command.UserId, cancellationToken))!;

        // Accept the invitation (validates status + expiry, raises InvitationAcceptedEvent)
        invitation.Accept(command.UserId, timeProvider.GetUtcNow());

        // Create FamilyMember record
        var member = FamilyMember.Create(invitation.FamilyId, command.UserId, invitation.Role);
        await memberRepository.AddAsync(member, cancellationToken);

        // Assign user to family
        user.AssignToFamily(invitation.FamilyId);

        return new AcceptInvitationResult(invitation.FamilyId, member.Id);
    }
}
