using System.Text.Json;
using FamilyHub.Modules.UserProfile.Domain.Events.EventSourcing;
using FamilyHub.Modules.UserProfile.Domain.Repositories;
using FamilyHub.Modules.UserProfile.Domain.ValueObjects;
using FamilyHub.Modules.UserProfile.Persistence.Entities;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Modules.UserProfile.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the profile event store.
/// Handles persistence and retrieval of profile events for event sourcing.
/// </summary>
public sealed class ProfileEventStore : IProfileEventStore
{
    private readonly UserProfileDbContext _context;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Event type name constant for snapshot events.
    /// </summary>
    private const string SnapshotEventTypeName = nameof(ProfileSnapshotEvent);

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileEventStore"/> class.
    /// </summary>
    /// <param name="context">The database context for persistence.</param>
    public ProfileEventStore(UserProfileDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task AppendEventAsync(ProfileEvent @event, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(@event);
        await _context.ProfileEvents.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AppendEventsAsync(IEnumerable<ProfileEvent> events, CancellationToken cancellationToken = default)
    {
        var entities = events.Select(MapToEntity);
        await _context.ProfileEvents.AddRangeAsync(entities, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProfileEvent>> GetEventsAsync(
        UserProfileId profileId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.ProfileEvents
            .Where(e => e.ProfileId == profileId.Value)
            .OrderBy(e => e.Version)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProfileEvent>> GetEventsFromVersionAsync(
        UserProfileId profileId,
        int fromVersion,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.ProfileEvents
            .Where(e => e.ProfileId == profileId.Value && e.Version > fromVersion)
            .OrderBy(e => e.Version)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToList();
    }

    /// <inheritdoc />
    public async Task<ProfileSnapshotEvent?> GetLatestSnapshotAsync(
        UserProfileId profileId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.ProfileEvents
            .Where(e => e.ProfileId == profileId.Value && e.EventType == SnapshotEventTypeName)
            .OrderByDescending(e => e.Version)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return null;
        }

        return MapToDomain(entity) as ProfileSnapshotEvent;
    }

    /// <inheritdoc />
    public async Task<int> GetCurrentVersionAsync(
        UserProfileId profileId,
        CancellationToken cancellationToken = default)
    {
        var maxVersion = await _context.ProfileEvents
            .Where(e => e.ProfileId == profileId.Value)
            .MaxAsync(e => (int?)e.Version, cancellationToken);

        return maxVersion ?? 0;
    }

    /// <inheritdoc />
    public async Task<bool> HasEventsAsync(
        UserProfileId profileId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ProfileEvents
            .AnyAsync(e => e.ProfileId == profileId.Value, cancellationToken);
    }

    /// <summary>
    /// Maps a domain ProfileEvent to a ProfileEventEntity for persistence.
    /// </summary>
    private static ProfileEventEntity MapToEntity(ProfileEvent @event)
    {
        var eventType = @event.GetType().Name;
        var eventData = SerializeEvent(@event);

        return new ProfileEventEntity
        {
            Id = @event.EventId,
            ProfileId = @event.ProfileId.Value,
            EventType = eventType,
            EventData = eventData,
            ChangedBy = @event.ChangedBy.Value,
            OccurredAt = @event.OccurredAt,
            Version = @event.Version
        };
    }

    /// <summary>
    /// Maps a ProfileEventEntity to the appropriate domain ProfileEvent.
    /// </summary>
    private static ProfileEvent MapToDomain(ProfileEventEntity entity)
    {
        return entity.EventType switch
        {
            nameof(ProfileCreatedEvent) => DeserializeEvent<ProfileCreatedEvent>(entity),
            nameof(ProfileFieldUpdatedEvent) => DeserializeEvent<ProfileFieldUpdatedEvent>(entity),
            nameof(ProfileSnapshotEvent) => DeserializeEvent<ProfileSnapshotEvent>(entity),
            _ => throw new InvalidOperationException($"Unknown event type: {entity.EventType}")
        };
    }

    /// <summary>
    /// Serializes a ProfileEvent to JSON, converting Vogen value objects to primitives.
    /// </summary>
    private static string SerializeEvent(ProfileEvent @event)
    {
        // Create a dictionary with primitive values for JSON storage
        var eventData = @event switch
        {
            ProfileCreatedEvent created => new Dictionary<string, object?>
            {
                ["eventId"] = created.EventId,
                ["profileId"] = created.ProfileId.Value,
                ["changedBy"] = created.ChangedBy.Value,
                ["occurredAt"] = created.OccurredAt,
                ["version"] = created.Version,
                ["userId"] = created.UserId.Value,
                ["displayName"] = created.DisplayName
            },
            ProfileFieldUpdatedEvent updated => new Dictionary<string, object?>
            {
                ["eventId"] = updated.EventId,
                ["profileId"] = updated.ProfileId.Value,
                ["changedBy"] = updated.ChangedBy.Value,
                ["occurredAt"] = updated.OccurredAt,
                ["version"] = updated.Version,
                ["fieldName"] = updated.FieldName,
                ["oldValue"] = updated.OldValue,
                ["newValue"] = updated.NewValue
            },
            ProfileSnapshotEvent snapshot => new Dictionary<string, object?>
            {
                ["eventId"] = snapshot.EventId,
                ["profileId"] = snapshot.ProfileId.Value,
                ["changedBy"] = snapshot.ChangedBy.Value,
                ["occurredAt"] = snapshot.OccurredAt,
                ["version"] = snapshot.Version,
                ["snapshotJson"] = snapshot.SnapshotJson
            },
            _ => throw new InvalidOperationException($"Unknown event type: {@event.GetType().Name}")
        };

        return JsonSerializer.Serialize(eventData, JsonOptions);
    }

    /// <summary>
    /// Deserializes a ProfileEvent from JSON, reconstructing Vogen value objects.
    /// </summary>
    private static T DeserializeEvent<T>(ProfileEventEntity entity) where T : ProfileEvent
    {
        using var document = JsonDocument.Parse(entity.EventData);
        var root = document.RootElement;

        var eventId = root.GetProperty("eventId").GetGuid();
        var profileId = UserProfileId.From(root.GetProperty("profileId").GetGuid());
        var changedBy = UserId.From(root.GetProperty("changedBy").GetGuid());
        var occurredAt = root.GetProperty("occurredAt").GetDateTime();
        var version = root.GetProperty("version").GetInt32();

        ProfileEvent result = typeof(T).Name switch
        {
            nameof(ProfileCreatedEvent) => new ProfileCreatedEvent(
                eventId,
                profileId,
                changedBy,
                occurredAt,
                version,
                UserId.From(root.GetProperty("userId").GetGuid()),
                root.GetProperty("displayName").GetString() ?? string.Empty
            ),
            nameof(ProfileFieldUpdatedEvent) => new ProfileFieldUpdatedEvent(
                eventId,
                profileId,
                changedBy,
                occurredAt,
                version,
                root.GetProperty("fieldName").GetString() ?? string.Empty,
                root.TryGetProperty("oldValue", out var oldVal) && oldVal.ValueKind != JsonValueKind.Null
                    ? oldVal.GetString()
                    : null,
                root.TryGetProperty("newValue", out var newVal) && newVal.ValueKind != JsonValueKind.Null
                    ? newVal.GetString()
                    : null
            ),
            nameof(ProfileSnapshotEvent) => new ProfileSnapshotEvent(
                eventId,
                profileId,
                changedBy,
                occurredAt,
                version,
                root.GetProperty("snapshotJson").GetString() ?? string.Empty
            ),
            _ => throw new InvalidOperationException($"Cannot deserialize to type: {typeof(T).Name}")
        };

        return (T)result;
    }
}
