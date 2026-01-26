using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Application.Services.EventSourcing;

/// <summary>
/// Service for replaying profile events to reconstruct state.
/// Used for audit trail verification and point-in-time state queries.
/// </summary>
public interface IProfileEventReplayService
{
    /// <summary>
    /// Reconstructs the current profile state from events.
    /// Uses snapshots for optimization when available.
    /// </summary>
    /// <param name="profileId">The profile to reconstruct.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The reconstructed profile state.</returns>
    Task<ProfileStateDto> ReplayEventsAsync(
        UserProfileId profileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reconstructs the profile state at a specific point in time.
    /// Useful for audit trail queries.
    /// </summary>
    /// <param name="profileId">The profile to reconstruct.</param>
    /// <param name="asOf">The point in time to reconstruct state for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The reconstructed profile state at the specified time, or null if no events exist before that time.</returns>
    Task<ProfileStateDto?> ReplayEventsAtTimeAsync(
        UserProfileId profileId,
        DateTime asOf,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a snapshot if the number of events since the last snapshot exceeds the threshold.
    /// Should be called after recording events.
    /// </summary>
    /// <param name="profileId">The profile to potentially snapshot.</param>
    /// <param name="changedBy">The user who triggered this operation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if a snapshot was created, false otherwise.</returns>
    Task<bool> CreateSnapshotIfNeededAsync(
        UserProfileId profileId,
        UserId changedBy,
        CancellationToken cancellationToken = default);
}
