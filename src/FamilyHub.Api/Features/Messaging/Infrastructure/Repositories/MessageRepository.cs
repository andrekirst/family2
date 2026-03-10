using FamilyHub.Api.Common.Database;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.Messaging.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IMessageRepository.
/// Uses cursor-based pagination for efficient timeline queries.
/// </summary>
public sealed class MessageRepository(AppDbContext context) : IMessageRepository
{
    public async Task<Message?> GetByIdAsync(MessageId id, CancellationToken cancellationToken = default)
    {
        return await context.Messages.FindAsync([id], cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(MessageId id, CancellationToken cancellationToken = default)
    {
        return await context.Messages.AnyAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<List<Message>> GetByFamilyAsync(
        FamilyId familyId,
        int limit = 50,
        DateTime? before = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Messages
            .Where(m => m.FamilyId == familyId);

        if (before.HasValue)
        {
            query = query.Where(m => m.SentAt < before.Value);
        }

        return await query
            .OrderByDescending(m => m.SentAt)
            .Take(Math.Min(limit, 100))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Message>> GetByConversationAsync(
        ConversationId conversationId,
        int limit = 50,
        DateTime? before = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Messages
            .Where(m => m.ConversationId == conversationId);

        if (before.HasValue)
        {
            query = query.Where(m => m.SentAt < before.Value);
        }

        return await query
            .OrderByDescending(m => m.SentAt)
            .Take(Math.Min(limit, 100))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Message message, CancellationToken cancellationToken = default)
    {
        await context.Messages.AddAsync(message, cancellationToken);
    }
}
