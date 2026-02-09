using FamilyHub.Api.Common.Email;
using FamilyHub.Api.Common.Email.Templates;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.Events;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace FamilyHub.Api.Features.Family.Application.EventHandlers;

/// <summary>
/// Handles InvitationSentEvent by sending the invitation email.
/// Uses the plaintext token from the event to build the acceptance URL.
/// </summary>
public static class InvitationSentEventHandler
{
    public static async Task Handle(
        InvitationSentEvent @event,
        IEmailService emailService,
        IFamilyRepository familyRepository,
        IUserRepository userRepository,
        IConfiguration configuration,
        CancellationToken ct)
    {
        var family = await familyRepository.GetByIdAsync(@event.FamilyId, ct);
        var inviter = await userRepository.GetByIdAsync(@event.InvitedByUserId, ct);

        var familyName = family?.Name.Value ?? "Unknown Family";
        var inviterName = inviter?.Name.Value ?? "A family member";

        var frontendUrl = configuration["App:FrontendUrl"] ?? "http://localhost:4200";
        var acceptUrl = $"{frontendUrl}/invitation/accept?token={Uri.EscapeDataString(@event.PlaintextToken)}";

        var htmlBody = InvitationEmailTemplate.GenerateHtml(
            familyName, inviterName, @event.Role.Value, acceptUrl, @event.ExpiresAt);

        var textBody = InvitationEmailTemplate.GenerateText(
            familyName, inviterName, @event.Role.Value, acceptUrl, @event.ExpiresAt);

        await emailService.SendEmailAsync(
            @event.InviteeEmail.Value,
            $"You've been invited to join {familyName} on Family Hub",
            htmlBody,
            textBody,
            ct);
    }
}
