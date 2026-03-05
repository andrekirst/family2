using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Domain.Repositories;

namespace FamilyHub.TestCommon.Fakes;

public class FakeConversationRepository(List<Conversation>? existingConversations = null) : IConversationRepository
{
    private readonly List<Conversation> _conversations = existingConversations ?? [];
    public List<Conversation> AddedConversations { get; } = [];

    public Task<Conversation?> GetByIdAsync(ConversationId id, CancellationToken ct = default) =>
        Task.FromResult(_conversations.Concat(AddedConversations).FirstOrDefault(c => c.Id == id));

    public Task<Conversation?> GetFamilyConversationAsync(FamilyId familyId, CancellationToken ct = default) =>
        Task.FromResult(_conversations.Concat(AddedConversations).FirstOrDefault(c => c.FamilyId == familyId));

    public Task<List<Conversation>> GetByUserAsync(FamilyId familyId, UserId userId, CancellationToken ct = default) =>
        Task.FromResult(_conversations.Concat(AddedConversations)
            .Where(c => c.FamilyId == familyId)
            .ToList());

    public Task AddAsync(Conversation conversation, CancellationToken ct = default)
    {
        AddedConversations.Add(conversation);
        return Task.CompletedTask;
    }
}
