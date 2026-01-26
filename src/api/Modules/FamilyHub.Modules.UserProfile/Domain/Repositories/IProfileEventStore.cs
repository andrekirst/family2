using FamilyHub.Modules.UserProfile.Domain.Events.EventSourcing;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Domain.Repositories;

/// <summary>
/// Event store interface for profile event sourcing.
/// Provides methods for appending and retrieving events for audit trail and state reconstruction.
/// </summary>
public interface IProfileEventStore
{
    /// <summary>
    /// Appends a new event to the event store.
    /// </summary>
    /// <param name="event">The event to append.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AppendEventAsync(ProfileEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends multiple events in a single operation.
    /// Events are stored atomically.
    /// </summary>
    /// <param name="events">The events to append.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AppendEventsAsync(IEnumerable<ProfileEvent> events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all events for a profile, ordered by version ascending.
    /// </summary>
    /// <param name="profileId">The profile to get events for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All events for the profile in version order.</returns>
    Task<IReadOnlyList<ProfileEvent>> GetEventsAsync(
        UserProfileId profileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets events for a profile starting from a specific version (exclusive).
    /// Useful for loading events after a snapshot.
    /// </summary>
    /// <param name="profileId">The profile to get events for.</param>
    /// <param name="fromVersion">Events with version greater than this will be returned.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Events with version greater than fromVersion.</returns>
    Task<IReadOnlyList<ProfileEvent>> GetEventsFromVersionAsync(
        UserProfileId profileId,
        int fromVersion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest snapshot event for a profile.
    /// </summary>
    /// <param name="profileId">The profile to get the snapshot for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The latest snapshot, or null if no snapshots exist.</returns>
    Task<ProfileSnapshotEvent?> GetLatestSnapshotAsync(
        UserProfileId profileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current (highest) version number for a profile.
    /// </summary>
    /// <param name="profileId">The profile to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The current version number, or 0 if no events exist.</returns>
    Task<int> GetCurrentVersionAsync(
        UserProfileId profileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any events exist for a profile.
    /// Used for backward compatibility migration of existing profiles.
    /// </summary>
    /// <param name="profileId">The profile to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if events exist, false otherwise.</returns>
    Task<bool> HasEventsAsync(
        UserProfileId profileId,
        CancellationToken cancellationToken = default);
}
