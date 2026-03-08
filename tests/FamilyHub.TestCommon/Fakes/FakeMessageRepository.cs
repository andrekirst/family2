using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Domain.Repositories;

namespace FamilyHub.TestCommon.Fakes;

public class FakeMessageRepository(List<Message>? existingMessages = null) : IMessageRepository
{
    private readonly List<Message> _messages = existingMessages ?? [];
    public List<Message> AddedMessages { get; } = [];

    public Task<Message?> GetByIdAsync(MessageId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_messages.Concat(AddedMessages).FirstOrDefault(m => m.Id == id));

    public Task<bool> ExistsByIdAsync(MessageId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_messages.Concat(AddedMessages).Any(m => m.Id == id));

    public Task<List<Message>> GetByFamilyAsync(
        FamilyId familyId,
        int limit = 50,
        DateTime? before = null,
        CancellationToken cancellationToken = default)
    {
        var query = _messages.Concat(AddedMessages)
            .Where(m => m.FamilyId == familyId);

        if (before.HasValue)
        {
            query = query.Where(m => m.SentAt < before.Value);
        }

        var result = query
            .OrderByDescending(m => m.SentAt)
            .Take(Math.Min(limit, 100))
            .ToList();

        return Task.FromResult(result);
    }

    public Task<List<Message>> GetByConversationAsync(
        ConversationId conversationId,
        int limit = 50,
        DateTime? before = null,
        CancellationToken cancellationToken = default)
    {
        var query = _messages.Concat(AddedMessages)
            .Where(m => m.ConversationId == conversationId);

        if (before.HasValue)
        {
            query = query.Where(m => m.SentAt < before.Value);
        }

        var result = query
            .OrderByDescending(m => m.SentAt)
            .Take(Math.Min(limit, 100))
            .ToList();

        return Task.FromResult(result);
    }

    public Task AddAsync(Message message, CancellationToken cancellationToken = default)
    {
        AddedMessages.Add(message);
        return Task.CompletedTask;
    }
}
