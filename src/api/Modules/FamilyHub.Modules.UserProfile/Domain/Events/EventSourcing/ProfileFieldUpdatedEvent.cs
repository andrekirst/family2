using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Domain.Events.EventSourcing;

/// <summary>
/// Event recorded when a profile field is updated.
/// Captures the field name and both old and new values for audit purposes.
/// </summary>
/// <param name="EventId">Unique identifier for this event instance.</param>
/// <param name="ProfileId">The profile being updated.</param>
/// <param name="ChangedBy">The user who made the change.</param>
/// <param name="OccurredAt">When the change occurred.</param>
/// <param name="Version">Sequential version number.</param>
/// <param name="FieldName">Name of the field that was updated (e.g., "DisplayName", "Birthday").</param>
/// <param name="OldValue">The previous value (JSON serialized for complex types, null if not set).</param>
/// <param name="NewValue">The new value (JSON serialized for complex types, null if clearing).</param>
public sealed record ProfileFieldUpdatedEvent(
    Guid EventId,
    UserProfileId ProfileId,
    UserId ChangedBy,
    DateTime OccurredAt,
    int Version,
    string FieldName,
    string? OldValue,
    string? NewValue
) : ProfileEvent(EventId, ProfileId, ChangedBy, OccurredAt, Version);
