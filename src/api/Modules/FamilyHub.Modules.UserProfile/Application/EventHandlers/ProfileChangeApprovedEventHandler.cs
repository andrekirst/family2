using FamilyHub.Modules.UserProfile.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.UserProfile.Application.EventHandlers;

/// <summary>
/// Handles ProfileChangeApprovedEvent to notify the child user when their change is approved.
/// </summary>
/// <remarks>
/// <para>
/// This handler logs the event and can be extended to send email notifications
/// to the child user when their profile change request is approved.
/// </para>
/// </remarks>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class ProfileChangeApprovedEventHandler(
    ILogger<ProfileChangeApprovedEventHandler> logger)
    : INotificationHandler<ProfileChangeApprovedEvent>
{
    /// <inheritdoc />
    public Task Handle(ProfileChangeApprovedEvent notification, CancellationToken cancellationToken)
    {
        LogProfileChangeApproved(
            notification.RequestId.Value,
            notification.ProfileId.Value,
            notification.ApprovedBy.Value,
            notification.FieldName,
            notification.NewValue);

        // TODO: Send email notification to the child user informing them their change was approved
        // This will be implemented when cross-module notification infrastructure is established.

        return Task.CompletedTask;
    }

    [LoggerMessage(
        LogLevel.Information,
        "Profile change approved: RequestId={RequestId}, ProfileId={ProfileId}, ApprovedBy={ApprovedBy}, " +
        "FieldName={FieldName}, NewValue={NewValue}")]
    partial void LogProfileChangeApproved(
        Guid requestId,
        Guid profileId,
        Guid approvedBy,
        string fieldName,
        string newValue);
}
