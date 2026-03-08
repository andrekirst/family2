using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.Entities;

namespace FamilyHub.Api.Features.Messaging.Domain.Repositories;

/// <summary>
/// Repository interface for Conversation aggregate.
/// </summary>
public interface IConversationRepository : IWriteRepository<Conversation, ConversationId>
{
    Task<Conversation?> GetFamilyConversationAsync(FamilyId familyId, CancellationToken ct = default);
    Task<List<Conversation>> GetByUserAsync(FamilyId familyId, UserId userId, CancellationToken ct = default);
}
