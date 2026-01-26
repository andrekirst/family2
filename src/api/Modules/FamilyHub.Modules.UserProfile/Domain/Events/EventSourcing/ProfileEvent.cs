using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Domain.Events.EventSourcing;

/// <summary>
/// Base class for profile event sourcing events.
/// These events are used for audit trail persistence and state reconstruction,
/// different from DomainEvent (MediatR) which is used for cross-module communication.
/// </summary>
/// <param name="EventId">Unique identifier for this event instance.</param>
/// <param name="ProfileId">The profile this event belongs to.</param>
/// <param name="ChangedBy">The user who initiated this change.</param>
/// <param name="OccurredAt">When this event occurred.</param>
/// <param name="Version">Sequential version number for ordering events.</param>
public abstract record ProfileEvent(
    Guid EventId,
    UserProfileId ProfileId,
    UserId ChangedBy,
    DateTime OccurredAt,
    int Version
);
