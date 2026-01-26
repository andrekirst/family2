using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using UserProfileAggregate = FamilyHub.Modules.UserProfile.Domain.Aggregates.UserProfile;

namespace FamilyHub.Modules.UserProfile.Application.Services.EventSourcing;

/// <summary>
/// Service for recording profile changes as events.
/// Used by command handlers to persist audit trail alongside state changes.
/// </summary>
public interface IProfileEventRecorder
{
    /// <summary>
    /// Records a profile creation event.
    /// Should be called after successfully creating a new profile.
    /// </summary>
    /// <param name="profile">The created profile.</param>
    /// <param name="createdBy">The user who created the profile.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordCreatedAsync(
        UserProfileAggregate profile,
        UserId createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a field update event with old and new values.
    /// Should be called after successfully updating a profile field.
    /// </summary>
    /// <param name="profileId">The profile being updated.</param>
    /// <param name="changedBy">The user who made the change.</param>
    /// <param name="fieldName">The name of the field that was updated.</param>
    /// <param name="oldValue">The previous value (serialized to string, null if not set).</param>
    /// <param name="newValue">The new value (serialized to string, null if clearing).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordFieldUpdateAsync(
        UserProfileId profileId,
        UserId changedBy,
        string fieldName,
        string? oldValue,
        string? newValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures that events exist for a profile.
    /// For backward compatibility: creates synthetic creation event for existing profiles without events.
    /// Should be called before recording update events for profiles that may predate event sourcing.
    /// </summary>
    /// <param name="profile">The profile to ensure events for.</param>
    /// <param name="changedBy">The user who triggered this operation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task EnsureEventsExistAsync(
        UserProfileAggregate profile,
        UserId changedBy,
        CancellationToken cancellationToken = default);
}
