using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.Entities;

namespace FamilyHub.Api.Features.Messaging.Domain.Repositories;

/// <summary>
/// Repository interface for Message aggregate.
/// Supports cursor-based pagination via the 'before' timestamp parameter.
/// </summary>
public interface IMessageRepository
{
    /// <summary>
    /// Get message by its unique identifier.
    /// </summary>
    Task<Message?> GetByIdAsync(MessageId id, CancellationToken ct = default);

    /// <summary>
    /// Get messages for a family channel with cursor pagination.
    /// Returns messages ordered by SentAt descending, limited by count.
    /// </summary>
    /// <param name="familyId">The family whose messages to retrieve</param>
    /// <param name="limit">Max number of messages to return (capped at 100)</param>
    /// <param name="before">Optional cursor: only return messages sent before this timestamp</param>
    /// <param name="ct">Cancellation token</param>
    Task<List<Message>> GetByFamilyAsync(FamilyId familyId, int limit = 50, DateTime? before = null, CancellationToken ct = default);

    /// <summary>
    /// Add a new message to the repository.
    /// </summary>
    Task AddAsync(Message message, CancellationToken ct = default);

    /// <summary>
    /// Save all pending changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
