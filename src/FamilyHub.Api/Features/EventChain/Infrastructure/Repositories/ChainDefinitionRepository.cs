using FamilyHub.Api.Common.Database;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.EventChain.Infrastructure.Repositories;

public sealed class ChainDefinitionRepository(AppDbContext context) : IChainDefinitionRepository
{
    public async Task<ChainDefinition?> GetByIdAsync(ChainDefinitionId id, CancellationToken cancellationToken = default)
    {
        return await context.ChainDefinitions.FindAsync([id], cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(ChainDefinitionId id, CancellationToken cancellationToken = default)
    {
        return await context.ChainDefinitions.AnyAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<ChainDefinition?> GetByIdWithStepsAsync(ChainDefinitionId id, CancellationToken cancellationToken = default)
    {
        return await context.ChainDefinitions
            .Include(d => d.Steps)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ChainDefinition>> GetByFamilyIdAsync(
        FamilyId familyId, bool? isEnabled = null, CancellationToken cancellationToken = default)
    {
        var query = context.ChainDefinitions
            .Include(d => d.Steps)
            .AsSplitQuery()
            .Where(d => d.FamilyId == familyId);

        if (isEnabled.HasValue)
        {
            query = query.Where(d => d.IsEnabled == isEnabled.Value);
        }

        return await query.OrderBy(d => d.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ChainDefinition>> GetEnabledByTriggerEventTypeAsync(
        string triggerEventType, CancellationToken cancellationToken = default)
    {
        return await context.ChainDefinitions
            .Include(d => d.Steps)
            .AsSplitQuery()
            .Where(d => d.IsEnabled && d.TriggerEventType == triggerEventType)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ChainDefinition>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return await context.ChainDefinitions
            .Include(d => d.Steps)
            .AsSplitQuery()
            .Where(d => d.IsTemplate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ChainDefinition definition, CancellationToken cancellationToken = default)
    {
        await context.ChainDefinitions.AddAsync(definition, cancellationToken);
    }

    public Task UpdateAsync(ChainDefinition definition, CancellationToken cancellationToken = default)
    {
        // EF Core change tracker detects modifications automatically
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ChainDefinition definition, CancellationToken cancellationToken = default)
    {
        context.ChainDefinitions.Remove(definition);
        return Task.CompletedTask;
    }
}
