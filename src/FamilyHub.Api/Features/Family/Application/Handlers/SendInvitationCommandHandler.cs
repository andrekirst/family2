using System.Security.Cryptography;
using System.Text;
using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Features.Family.Application.Commands;
using FamilyHub.Api.Features.Family.Application.Services;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Handlers;

/// <summary>
/// Handler for SendInvitationCommand.
/// Validates authorization, checks for duplicates, generates a secure token,
/// creates the invitation, and persists it (which triggers the email via domain event).
/// </summary>
public static class SendInvitationCommandHandler
{
    public static async Task<SendInvitationResult> Handle(
        SendInvitationCommand command,
        FamilyAuthorizationService authService,
        IFamilyInvitationRepository invitationRepository,
        IFamilyMemberRepository memberRepository,
        CancellationToken ct)
    {
        // Authorization: only Owner/Admin can invite
        if (!await authService.CanInviteAsync(command.InvitedBy, command.FamilyId, ct))
        {
            throw new DomainException("You do not have permission to send invitations for this family");
        }

        // Check for duplicate pending invitation
        var existing = await invitationRepository.GetByEmailAndFamilyAsync(command.InviteeEmail, command.FamilyId, ct);
        if (existing is not null)
        {
            throw new DomainException("An invitation has already been sent to this email for this family");
        }

        // Check if user is already a member (by checking all members' emails would require user lookup)
        // For now we rely on the accept flow to prevent duplicate membership

        // Generate secure token
        var plaintextToken = GenerateSecureToken();
        var tokenHash = ComputeSha256Hash(plaintextToken);

        // Create invitation aggregate (raises InvitationSentEvent with plaintext token)
        var invitation = FamilyInvitation.Create(
            command.FamilyId,
            command.InvitedBy,
            command.InviteeEmail,
            command.Role,
            InvitationToken.From(tokenHash),
            plaintextToken);

        await invitationRepository.AddAsync(invitation, ct);
        await invitationRepository.SaveChangesAsync(ct);

        return new SendInvitationResult(invitation.Id);
    }

    /// <summary>
    /// Generates a 64-character URL-safe cryptographically random token.
    /// </summary>
    private static string GenerateSecureToken()
    {
        var bytes = new byte[48]; // 48 bytes = 64 base64url chars
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    /// <summary>
    /// Computes SHA256 hash of a string and returns it as a 64-character hex string.
    /// </summary>
    internal static string ComputeSha256Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }
}
