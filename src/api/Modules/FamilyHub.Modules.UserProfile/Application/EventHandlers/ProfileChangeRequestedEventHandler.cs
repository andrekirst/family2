using FamilyHub.Modules.UserProfile.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.UserProfile.Application.EventHandlers;

/// <summary>
/// Handles ProfileChangeRequestedEvent to notify parents when a child requests a profile change.
/// </summary>
/// <remarks>
/// <para>
/// This handler logs the event and can be extended to send email notifications
/// to Owner/Admin users in the family when cross-module notification infrastructure
/// is properly established.
/// </para>
/// <para>
/// TODO: Implement email notification to parents using shared notification infrastructure.
/// - Get all Owner/Admin users in the family
/// - Render notification email template
/// - Create email outbox entries
/// </para>
/// </remarks>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class ProfileChangeRequestedEventHandler(
    ILogger<ProfileChangeRequestedEventHandler> logger)
    : INotificationHandler<ProfileChangeRequestedEvent>
{
    /// <inheritdoc />
    public Task Handle(ProfileChangeRequestedEvent notification, CancellationToken cancellationToken)
    {
        LogProfileChangeRequested(
            notification.RequestId.Value,
            notification.ProfileId.Value,
            notification.RequestedBy.Value,
            notification.FamilyId.Value,
            notification.FieldName,
            notification.NewValue);

        // TODO: Send email notification to parents (Owner/Admin users in the family)
        // This will be implemented when cross-module notification infrastructure is established.
        // For now, the event is logged for audit trail.

        return Task.CompletedTask;
    }

    [LoggerMessage(
        LogLevel.Information,
        "Profile change requested: RequestId={RequestId}, ProfileId={ProfileId}, RequestedBy={RequestedBy}, " +
        "FamilyId={FamilyId}, FieldName={FieldName}, NewValue={NewValue}")]
    partial void LogProfileChangeRequested(
        Guid requestId,
        Guid profileId,
        Guid requestedBy,
        Guid familyId,
        string fieldName,
        string newValue);
}
