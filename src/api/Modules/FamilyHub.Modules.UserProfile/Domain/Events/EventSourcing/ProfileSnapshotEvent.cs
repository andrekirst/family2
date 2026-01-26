using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Domain.Events.EventSourcing;

/// <summary>
/// Event that captures a complete snapshot of the profile state.
/// Used for performance optimization - replay starts from the latest snapshot
/// instead of replaying all events from the beginning.
/// </summary>
/// <remarks>
/// Snapshots are created automatically after a configurable number of events
/// (default: 50) to optimize event replay performance.
/// </remarks>
/// <param name="EventId">Unique identifier for this event instance.</param>
/// <param name="ProfileId">The profile this snapshot represents.</param>
/// <param name="ChangedBy">The user who triggered the snapshot creation.</param>
/// <param name="OccurredAt">When the snapshot was created.</param>
/// <param name="Version">Sequential version number.</param>
/// <param name="SnapshotJson">JSON-serialized complete profile state.</param>
public sealed record ProfileSnapshotEvent(
    Guid EventId,
    UserProfileId ProfileId,
    UserId ChangedBy,
    DateTime OccurredAt,
    int Version,
    string SnapshotJson
) : ProfileEvent(EventId, ProfileId, ChangedBy, OccurredAt, Version);
