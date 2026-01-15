using FamilyHub.Infrastructure.Email;
using FamilyHub.Infrastructure.Email.Models;
using FamilyHub.Modules.Family.Domain;
using FamilyHub.Modules.Family.Domain.Abstractions;
using FamilyHub.Modules.Family.Domain.Events;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Family.Persistence.Repositories;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Family.Application.Handlers;

/// <summary>
/// Handler for FamilyMemberInvitedEvent.
/// Creates EmailOutbox entries for sending invitation emails to invited family members.
/// Uses IUserLookupService for proper bounded context separation.
/// </summary>
/// <param name="familyRepository">Repository for family data access.</param>
/// <param name="emailOutboxRepository">Repository for email outbox management.</param>
/// <param name="emailTemplateService">Service for rendering email templates.</param>
/// <param name="userLookupService">Service for cross-module user lookups.</param>
/// <param name="familyUnitOfWork">Unit of work for Family database transactions.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class FamilyMemberInvitedEventHandler(
    IFamilyRepository familyRepository,
    IEmailOutboxRepository emailOutboxRepository,
    IEmailTemplateService emailTemplateService,
    IUserLookupService userLookupService,
    IFamilyUnitOfWork familyUnitOfWork,
    ILogger<FamilyMemberInvitedEventHandler> logger)
    : INotificationHandler<FamilyMemberInvitedEvent>
{
    private const string FrontendBaseUrl = "http://localhost:4200"; // TODO: Move to configuration

    /// <inheritdoc />
    public async Task Handle(FamilyMemberInvitedEvent notification, CancellationToken cancellationToken)
    {
        LogProcessingEvent(notification.InvitationId.Value, notification.Email.Value);

        try
        {
            // 1. Get family name
            var family = await familyRepository.GetByIdAsync(notification.FamilyId, cancellationToken);
            if (family == null)
            {
                LogFamilyNotFound(notification.FamilyId.Value, notification.InvitationId.Value);
                return; // Skip email if family doesn't exist
            }

            // 2. Get inviter's email using IUserLookupService
            var inviterEmail = await userLookupService.GetUserEmailAsync(notification.InvitedByUserId, cancellationToken);

            if (inviterEmail == null)
            {
                LogInviterNotFound(notification.InvitedByUserId.Value, notification.InvitationId.Value);
                return; // Skip email if inviter doesn't exist
            }

            // 3. Build invitation URL
            var invitationUrl = $"{FrontendBaseUrl}/accept-invitation?token={notification.Token.Value}";

            // 4. Render email template
            var emailModel = new InvitationEmailModel
            {
                InviterName = inviterEmail.Value.Value, // Using email as name (temporary - could extend to get full name)
                FamilyName = family.Name.Value,
                InvitationUrl = invitationUrl,
                ExpiresAt = notification.ExpiresAt
            };

            var htmlBody = await emailTemplateService.RenderTemplateAsync(
                "InvitationEmail",
                emailModel,
                cancellationToken);

            // 5. Create email outbox entry
            var emailOutbox = EmailOutbox.Create(
                outboxEventId: OutboxEventId.New(),
                to: notification.Email.Value,
                toName: notification.Email.Value, // Using email as name
                subject: $"{inviterEmail.Value.Value} invited you to join their family on Family Hub",
                htmlBody: htmlBody);

            await emailOutboxRepository.AddAsync(emailOutbox, cancellationToken);
            await familyUnitOfWork.SaveChangesAsync(cancellationToken);

            LogEmailCreated(notification.InvitationId.Value, notification.Email.Value, emailOutbox.Id.Value);
        }
        catch (Exception ex)
        {
            LogErrorCreatingEmail(ex, notification.InvitationId.Value, notification.Email.Value);
            throw; // Re-throw to ensure domain event processing fails
        }
    }

    #region Logging

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Processing FamilyMemberInvitedEvent for invitation {InvitationId}, email {Email}")]
    private partial void LogProcessingEvent(Guid invitationId, string email);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "Family {FamilyId} not found while processing invitation {InvitationId}")]
    private partial void LogFamilyNotFound(Guid familyId, Guid invitationId);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "Inviter {InvitedByUserId} not found while processing invitation {InvitationId}")]
    private partial void LogInviterNotFound(Guid invitedByUserId, Guid invitationId);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Email outbox entry {EmailOutboxId} created for invitation {InvitationId}, recipient {Email}")]
    private partial void LogEmailCreated(Guid invitationId, string email, Guid emailOutboxId);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Error,
        Message = "Error creating email outbox entry for invitation {InvitationId}, recipient {Email}")]
    private partial void LogErrorCreatingEmail(Exception ex, Guid invitationId, string email);

    #endregion
}
