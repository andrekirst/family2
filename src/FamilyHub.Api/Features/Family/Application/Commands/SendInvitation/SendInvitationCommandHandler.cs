using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Common.Infrastructure.Security;
using FamilyHub.Api.Features.Family.Application.Services;
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
    FamilyAuthorizationService authService,
    IFamilyInvitationRepository invitationRepository)
    : ICommandHandler<SendInvitationCommand, SendInvitationResult>
{
    public async ValueTask<SendInvitationResult> Handle(
        SendInvitationCommand command,
        CancellationToken cancellationToken)
    {
        // Authorization: only Owner/Admin can invite
        if (!await authService.CanInviteAsync(command.InvitedBy, command.FamilyId, cancellationToken))
        {
            throw new DomainException("You do not have permission to send invitations for this family");
        }

        // Check for duplicate pending invitation
        var existing = await invitationRepository.GetByEmailAndFamilyAsync(command.InviteeEmail, command.FamilyId, cancellationToken);
        if (existing is not null)
        {
            throw new DomainException("An invitation has already been sent to this email for this family");
        }

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
