using FamilyHub.Modules.UserProfile.Domain.Events.EventSourcing;
using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using UserProfileAggregate = FamilyHub.Modules.UserProfile.Domain.Aggregates.UserProfile;

namespace FamilyHub.Modules.UserProfile.Application.Services.EventSourcing;

/// <summary>
/// Service for recording profile changes as events.
/// Implements the dual-write pattern: events are recorded alongside state changes.
/// </summary>
public sealed class ProfileEventRecorder : IProfileEventRecorder
{
    private readonly IProfileEventStore _eventStore;
    private readonly IProfileEventReplayService _replayService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileEventRecorder"/> class.
    /// </summary>
    /// <param name="eventStore">The event store for persisting events.</param>
    /// <param name="replayService">The replay service for snapshot creation.</param>
    public ProfileEventRecorder(
        IProfileEventStore eventStore,
        IProfileEventReplayService replayService)
    {
        _eventStore = eventStore;
        _replayService = replayService;
    }

    /// <inheritdoc />
    public async Task RecordCreatedAsync(
        UserProfileAggregate profile,
        UserId createdBy,
        CancellationToken cancellationToken = default)
    {
        var @event = new ProfileCreatedEvent(
            Guid.NewGuid(),
            profile.Id,
            createdBy,
            DateTime.UtcNow,
            1, // First event is always version 1
            profile.UserId,
            profile.DisplayName.Value
        );

        await _eventStore.AppendEventAsync(@event, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RecordFieldUpdateAsync(
        UserProfileId profileId,
        UserId changedBy,
        string fieldName,
        string? oldValue,
        string? newValue,
        CancellationToken cancellationToken = default)
    {
        var currentVersion = await _eventStore.GetCurrentVersionAsync(profileId, cancellationToken);

        var @event = new ProfileFieldUpdatedEvent(
            Guid.NewGuid(),
            profileId,
            changedBy,
            DateTime.UtcNow,
            currentVersion + 1,
            fieldName,
            oldValue,
            newValue
        );

        await _eventStore.AppendEventAsync(@event, cancellationToken);

        // Check if we need to create a snapshot
        await _replayService.CreateSnapshotIfNeededAsync(profileId, changedBy, cancellationToken);
    }

    /// <inheritdoc />
    public async Task EnsureEventsExistAsync(
        UserProfileAggregate profile,
        UserId changedBy,
        CancellationToken cancellationToken = default)
    {
        var hasEvents = await _eventStore.HasEventsAsync(profile.Id, cancellationToken);

        if (!hasEvents)
        {
            // Create synthetic creation event for existing profile (backward compatibility)
            await RecordCreatedAsync(profile, changedBy, cancellationToken);
        }
    }
}
