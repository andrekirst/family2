using FamilyHub.Api.Common.Application;
using FamilyHub.Api.Common.Email;
using FamilyHub.Api.Common.Email.Templates;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.Events;
using FamilyHub.Api.Features.Family.Domain.Repositories;

namespace FamilyHub.Api.Features.Family.Application.EventHandlers;

/// <summary>
/// Handles InvitationSentEvent by sending the invitation email.
/// Uses the plaintext token from the event to build the acceptance URL.
/// </summary>
public sealed class InvitationSentEventHandler(
    IEmailService emailService,
    IFamilyRepository familyRepository,
    IUserRepository userRepository,
    IConfiguration configuration)
    : IDomainEventHandler<InvitationSentEvent>
{
    public async ValueTask Handle(
        InvitationSentEvent @event,
        CancellationToken cancellationToken)
    {
        var family = await familyRepository.GetByIdAsync(@event.FamilyId, cancellationToken);
        var inviter = await userRepository.GetByIdAsync(@event.InvitedByUserId, cancellationToken);

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
            cancellationToken);
    }
}
