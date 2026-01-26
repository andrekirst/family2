using System.Text.Json;
using FamilyHub.Modules.UserProfile.Domain.Events.EventSourcing;
using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;

namespace FamilyHub.Modules.UserProfile.Application.Services.EventSourcing;

/// <summary>
/// Service for replaying profile events to reconstruct state.
/// Implements snapshot optimization for performance.
/// </summary>
public sealed class ProfileEventReplayService : IProfileEventReplayService
{
    private readonly IProfileEventStore _eventStore;

    /// <summary>
    /// Number of events after which a snapshot should be created.
    /// </summary>
    public const int SnapshotThreshold = 50;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileEventReplayService"/> class.
    /// </summary>
    /// <param name="eventStore">The event store for retrieving events.</param>
    public ProfileEventReplayService(IProfileEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    /// <inheritdoc />
    public async Task<ProfileStateDto> ReplayEventsAsync(
        UserProfileId profileId,
        CancellationToken cancellationToken = default)
    {
        // Try to load from snapshot first for performance
        var snapshot = await _eventStore.GetLatestSnapshotAsync(profileId, cancellationToken);
        ProfileStateDto state;
        int fromVersion;

        if (snapshot != null)
        {
            // Start from snapshot
            state = JsonSerializer.Deserialize<ProfileStateDto>(snapshot.SnapshotJson, JsonOptions)
                ?? new ProfileStateDto { ProfileId = profileId };
            fromVersion = snapshot.Version;
        }
        else
        {
            // Start from scratch
            state = new ProfileStateDto { ProfileId = profileId };
            fromVersion = 0;
        }

        // Apply events since snapshot (or from beginning)
        var events = await _eventStore.GetEventsFromVersionAsync(profileId, fromVersion, cancellationToken);
        foreach (var @event in events)
        {
            ApplyEvent(state, @event);
        }

        return state;
    }

    /// <inheritdoc />
    public async Task<ProfileStateDto?> ReplayEventsAtTimeAsync(
        UserProfileId profileId,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        // Get all events up to the specified time
        var allEvents = await _eventStore.GetEventsAsync(profileId, cancellationToken);
        var eventsBeforeTime = allEvents.Where(e => e.OccurredAt <= asOf).ToList();

        if (eventsBeforeTime.Count == 0)
        {
            return null;
        }

        // Find the latest snapshot before the specified time
        var latestSnapshot = eventsBeforeTime
            .OfType<ProfileSnapshotEvent>()
            .LastOrDefault();

        ProfileStateDto state;
        IEnumerable<ProfileEvent> eventsToApply;

        if (latestSnapshot != null)
        {
            state = JsonSerializer.Deserialize<ProfileStateDto>(latestSnapshot.SnapshotJson, JsonOptions)
                ?? new ProfileStateDto { ProfileId = profileId };
            eventsToApply = eventsBeforeTime.Where(e => e.Version > latestSnapshot.Version);
        }
        else
        {
            state = new ProfileStateDto { ProfileId = profileId };
            eventsToApply = eventsBeforeTime;
        }

        // Apply events in order
        foreach (var @event in eventsToApply.OrderBy(e => e.Version))
        {
            ApplyEvent(state, @event);
        }

        return state;
    }

    /// <inheritdoc />
    public async Task<bool> CreateSnapshotIfNeededAsync(
        UserProfileId profileId,
        UserId changedBy,
        CancellationToken cancellationToken = default)
    {
        var currentVersion = await _eventStore.GetCurrentVersionAsync(profileId, cancellationToken);
        var snapshot = await _eventStore.GetLatestSnapshotAsync(profileId, cancellationToken);
        var eventsSinceSnapshot = currentVersion - (snapshot?.Version ?? 0);

        if (eventsSinceSnapshot >= SnapshotThreshold)
        {
            // Reconstruct state and create snapshot
            var state = await ReplayEventsAsync(profileId, cancellationToken);

            var snapshotEvent = new ProfileSnapshotEvent(
                Guid.NewGuid(),
                profileId,
                changedBy,
                DateTime.UtcNow,
                currentVersion + 1,
                JsonSerializer.Serialize(state, JsonOptions)
            );

            await _eventStore.AppendEventAsync(snapshotEvent, cancellationToken);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Applies an event to the state object.
    /// </summary>
    private static void ApplyEvent(ProfileStateDto state, ProfileEvent @event)
    {
        state.Version = @event.Version;
        state.LastModified = @event.OccurredAt;

        switch (@event)
        {
            case ProfileCreatedEvent created:
                ApplyCreatedEvent(state, created);
                break;

            case ProfileFieldUpdatedEvent updated:
                ApplyFieldUpdatedEvent(state, updated);
                break;

            case ProfileSnapshotEvent:
                // Snapshots don't modify state - they capture it
                break;
        }
    }

    /// <summary>
    /// Applies a creation event to the state.
    /// </summary>
    private static void ApplyCreatedEvent(ProfileStateDto state, ProfileCreatedEvent @event)
    {
        state.ProfileId = @event.ProfileId;
        state.UserId = @event.UserId;
        state.DisplayName = @event.DisplayName;
    }

    /// <summary>
    /// Applies a field update event to the state.
    /// </summary>
    private static void ApplyFieldUpdatedEvent(ProfileStateDto state, ProfileFieldUpdatedEvent @event)
    {
        switch (@event.FieldName)
        {
            case nameof(ProfileStateDto.DisplayName):
                state.DisplayName = @event.NewValue ?? string.Empty;
                break;

            case nameof(ProfileStateDto.Birthday):
                state.Birthday = string.IsNullOrEmpty(@event.NewValue)
                    ? null
                    : DateOnly.Parse(@event.NewValue);
                break;

            case nameof(ProfileStateDto.Pronouns):
                state.Pronouns = @event.NewValue;
                break;

            case nameof(ProfileStateDto.Language):
                state.Language = @event.NewValue ?? "en";
                break;

            case nameof(ProfileStateDto.Timezone):
                state.Timezone = @event.NewValue ?? "UTC";
                break;

            case nameof(ProfileStateDto.DateFormat):
                state.DateFormat = @event.NewValue ?? "yyyy-MM-dd";
                break;

            case nameof(ProfileStateDto.BirthdayVisibility):
                state.BirthdayVisibility = @event.NewValue ?? "family";
                break;

            case nameof(ProfileStateDto.PronounsVisibility):
                state.PronounsVisibility = @event.NewValue ?? "family";
                break;

            case nameof(ProfileStateDto.PreferencesVisibility):
                state.PreferencesVisibility = @event.NewValue ?? "hidden";
                break;

            // Handle combined preferences and visibility updates
            case "Preferences":
                ApplyPreferencesUpdate(state, @event.NewValue);
                break;

            case "FieldVisibility":
                ApplyFieldVisibilityUpdate(state, @event.NewValue);
                break;
        }
    }

    /// <summary>
    /// Applies a preferences update from JSON.
    /// </summary>
    private static void ApplyPreferencesUpdate(ProfileStateDto state, string? newValue)
    {
        if (string.IsNullOrEmpty(newValue))
        {
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(newValue);
            var root = document.RootElement;

            if (root.TryGetProperty("language", out var language))
            {
                state.Language = language.GetString() ?? "en";
            }

            if (root.TryGetProperty("timezone", out var timezone))
            {
                state.Timezone = timezone.GetString() ?? "UTC";
            }

            if (root.TryGetProperty("dateFormat", out var dateFormat))
            {
                state.DateFormat = dateFormat.GetString() ?? "yyyy-MM-dd";
            }
        }
        catch (JsonException)
        {
            // Invalid JSON - ignore
        }
    }

    /// <summary>
    /// Applies a field visibility update from JSON.
    /// </summary>
    private static void ApplyFieldVisibilityUpdate(ProfileStateDto state, string? newValue)
    {
        if (string.IsNullOrEmpty(newValue))
        {
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(newValue);
            var root = document.RootElement;

            if (root.TryGetProperty("birthdayVisibility", out var birthday))
            {
                state.BirthdayVisibility = birthday.GetString() ?? "family";
            }

            if (root.TryGetProperty("pronounsVisibility", out var pronouns))
            {
                state.PronounsVisibility = pronouns.GetString() ?? "family";
            }

            if (root.TryGetProperty("preferencesVisibility", out var preferences))
            {
                state.PreferencesVisibility = preferences.GetString() ?? "hidden";
            }
        }
        catch (JsonException)
        {
            // Invalid JSON - ignore
        }
    }
}
