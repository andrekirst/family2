using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.Security;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.SendInvitation;

/// <summary>
/// Handler for SendInvitationCommand.
/// Validates authorization, checks for duplicates, generates a secure token,
/// creates the invitation, and persists it (which triggers the email via domain event).
/// </summary>
public sealed class SendInvitationCommandHandler(
    IFamilyInvitationRepository invitationRepository)
    : ICommandHandler<SendInvitationCommand, SendInvitationResult>
{
    public async ValueTask<SendInvitationResult> Handle(
        SendInvitationCommand command,
        CancellationToken cancellationToken)
    {
        // Generate secure token
        var plaintextToken = SecureTokenHelper.GenerateSecureToken();
        var tokenHash = SecureTokenHelper.ComputeSha256Hash(plaintextToken);

        // Create invitation aggregate (raises InvitationSentEvent with plaintext token)
        var invitation = FamilyInvitation.Create(
            command.FamilyId,
            command.InvitedBy,
            command.InviteeEmail,
            command.Role,
            InvitationToken.From(tokenHash),
            plaintextToken);

        await invitationRepository.AddAsync(invitation, cancellationToken);

        return new SendInvitationResult(invitation.Id);
    }
}
