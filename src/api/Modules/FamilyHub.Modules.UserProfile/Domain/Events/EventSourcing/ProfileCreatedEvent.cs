using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Domain.Events.EventSourcing;

/// <summary>
/// Event recorded when a new user profile is created.
/// This is the initial event in a profile's event stream.
/// </summary>
/// <param name="EventId">Unique identifier for this event instance.</param>
/// <param name="ProfileId">The newly created profile's ID.</param>
/// <param name="ChangedBy">The user who created the profile.</param>
/// <param name="OccurredAt">When the profile was created.</param>
/// <param name="Version">Always 1 for creation events.</param>
/// <param name="UserId">The user ID that owns this profile.</param>
/// <param name="DisplayName">The initial display name for the profile.</param>
public sealed record ProfileCreatedEvent(
    Guid EventId,
    UserProfileId ProfileId,
    UserId ChangedBy,
    DateTime OccurredAt,
    int Version,
    UserId UserId,
    string DisplayName
) : ProfileEvent(EventId, ProfileId, ChangedBy, OccurredAt, Version);
