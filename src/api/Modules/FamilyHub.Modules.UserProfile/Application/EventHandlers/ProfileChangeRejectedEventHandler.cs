using FamilyHub.Modules.UserProfile.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.UserProfile.Application.EventHandlers;

/// <summary>
/// Handles ProfileChangeRejectedEvent to notify the child user when their change is rejected.
/// </summary>
/// <remarks>
/// <para>
/// This handler logs the event and can be extended to send email notifications
/// to the child user when their profile change request is rejected, including the reason.
/// </para>
/// </remarks>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class ProfileChangeRejectedEventHandler(
    ILogger<ProfileChangeRejectedEventHandler> logger)
    : INotificationHandler<ProfileChangeRejectedEvent>
{
    /// <inheritdoc />
    public Task Handle(ProfileChangeRejectedEvent notification, CancellationToken cancellationToken)
    {
        LogProfileChangeRejected(
            notification.RequestId.Value,
            notification.ProfileId.Value,
            notification.RejectedBy.Value,
            notification.FieldName,
            notification.Reason);

        // TODO: Send email notification to the child user informing them their change was rejected
        // Include the rejection reason so they understand why and can make corrections
        // This will be implemented when cross-module notification infrastructure is established.

        return Task.CompletedTask;
    }

    [LoggerMessage(
        LogLevel.Information,
        "Profile change rejected: RequestId={RequestId}, ProfileId={ProfileId}, RejectedBy={RejectedBy}, " +
        "FieldName={FieldName}, Reason={Reason}")]
    partial void LogProfileChangeRejected(
        Guid requestId,
        Guid profileId,
        Guid rejectedBy,
        string fieldName,
        string reason);
}
