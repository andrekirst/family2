using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.Entities;

namespace FamilyHub.Api.Features.Messaging.Domain.Repositories;

/// <summary>
/// Repository interface for Message aggregate.
/// Supports cursor-based pagination via the 'before' timestamp parameter.
/// </summary>
public interface IMessageRepository : IWriteRepository<Message, MessageId>
{
    /// <summary>
    /// Get messages for a family channel with cursor pagination.
    /// Returns messages ordered by SentAt descending, limited by count.
    /// </summary>
    /// <param name="familyId">The family whose messages to retrieve</param>
    /// <param name="limit">Max number of messages to return (capped at 100)</param>
    /// <param name="before">Optional cursor: only return messages sent before this timestamp</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<List<Message>> GetByFamilyAsync(FamilyId familyId, int limit = 50, DateTime? before = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get messages for a conversation with cursor pagination.
    /// </summary>
    Task<List<Message>> GetByConversationAsync(ConversationId conversationId, int limit = 50, DateTime? before = null, CancellationToken cancellationToken = default);
}
