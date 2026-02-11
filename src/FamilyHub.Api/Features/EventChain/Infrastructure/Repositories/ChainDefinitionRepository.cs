using FamilyHub.Api.Common.Database;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.EventChain.Infrastructure.Repositories;

public sealed class ChainDefinitionRepository(AppDbContext context) : IChainDefinitionRepository
{
    public async Task<ChainDefinition?> GetByIdAsync(ChainDefinitionId id, CancellationToken ct = default)
    {
        return await context.ChainDefinitions.FindAsync([id], cancellationToken: ct);
    }

    public async Task<ChainDefinition?> GetByIdWithStepsAsync(ChainDefinitionId id, CancellationToken ct = default)
    {
        return await context.ChainDefinitions
            .Include(d => d.Steps)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<IReadOnlyList<ChainDefinition>> GetByFamilyIdAsync(
        FamilyId familyId, bool? isEnabled = null, CancellationToken ct = default)
    {
        var query = context.ChainDefinitions
            .Include(d => d.Steps)
            .Where(d => d.FamilyId == familyId);

        if (isEnabled.HasValue)
            query = query.Where(d => d.IsEnabled == isEnabled.Value);

        return await query.OrderBy(d => d.CreatedAt).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ChainDefinition>> GetEnabledByTriggerEventTypeAsync(
        string triggerEventType, CancellationToken ct = default)
    {
        return await context.ChainDefinitions
            .Include(d => d.Steps)
            .Where(d => d.IsEnabled && d.TriggerEventType == triggerEventType)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ChainDefinition>> GetTemplatesAsync(CancellationToken ct = default)
    {
        return await context.ChainDefinitions
            .Include(d => d.Steps)
            .Where(d => d.IsTemplate)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ChainDefinition definition, CancellationToken ct = default)
    {
        await context.ChainDefinitions.AddAsync(definition, ct);
    }

    public Task UpdateAsync(ChainDefinition definition, CancellationToken ct = default)
    {
        context.ChainDefinitions.Update(definition);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ChainDefinition definition, CancellationToken ct = default)
    {
        context.ChainDefinitions.Remove(definition);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await context.SaveChangesAsync(ct);
    }
}
