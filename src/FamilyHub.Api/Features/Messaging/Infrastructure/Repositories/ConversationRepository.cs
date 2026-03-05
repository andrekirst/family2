using FamilyHub.Api.Common.Database;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.Entities;
using FamilyHub.Api.Features.Messaging.Domain.Repositories;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.Messaging.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IConversationRepository.
/// </summary>
public sealed class ConversationRepository(AppDbContext context) : IConversationRepository
{
    public async Task<Conversation?> GetByIdAsync(ConversationId id, CancellationToken ct = default)
    {
        return await context.Conversations.FindAsync([id], cancellationToken: ct);
    }

    public async Task<Conversation?> GetFamilyConversationAsync(FamilyId familyId, CancellationToken ct = default)
    {
        return await context.Conversations
            .FirstOrDefaultAsync(c => c.FamilyId == familyId && c.Type == ConversationType.Family, ct);
    }

    public async Task<List<Conversation>> GetByUserAsync(FamilyId familyId, UserId userId, CancellationToken ct = default)
    {
        return await context.Conversations
            .Where(c => c.FamilyId == familyId
                && c.Members.Any(m => m.UserId == userId && m.LeftAt == null))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Conversation conversation, CancellationToken ct = default)
    {
        await context.Conversations.AddAsync(conversation, ct);
    }
}
