using FamilyHub.Api.Common.Database;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Enums;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.EventChain.Infrastructure.Repositories;

public sealed class ChainExecutionRepository(AppDbContext context) : IChainExecutionRepository
{
    public async Task<ChainExecution?> GetByIdAsync(ChainExecutionId id, CancellationToken ct = default)
    {
        return await context.ChainExecutions.FindAsync([id], cancellationToken: ct);
    }

    public async Task<ChainExecution?> GetByIdWithStepsAsync(ChainExecutionId id, CancellationToken ct = default)
    {
        return await context.ChainExecutions
            .Include(e => e.StepExecutions)
            .Include(e => e.ChainDefinition)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<IReadOnlyList<ChainExecution>> GetByFamilyIdAsync(
        FamilyId familyId,
        ChainDefinitionId? chainDefinitionId = null,
        ChainExecutionStatus? status = null,
        CancellationToken ct = default)
    {
        var query = context.ChainExecutions
            .Include(e => e.StepExecutions)
            .Where(e => e.FamilyId == familyId);

        if (chainDefinitionId.HasValue)
            query = query.Where(e => e.ChainDefinitionId == chainDefinitionId.Value);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        return await query.OrderByDescending(e => e.StartedAt).ToListAsync(ct);
    }

    public async Task<StepExecution?> GetStepExecutionAsync(Guid stepExecutionId, CancellationToken ct = default)
    {
        return await context.StepExecutions.FindAsync([stepExecutionId], cancellationToken: ct);
    }

    public async Task<IReadOnlyList<ChainEntityMapping>> GetEntityMappingsAsync(
        Guid entityId, string? entityType = null, CancellationToken ct = default)
    {
        var query = context.ChainEntityMappings.Where(m => m.EntityId == entityId);

        if (entityType is not null)
            query = query.Where(m => m.EntityType == entityType);

        return await query.ToListAsync(ct);
    }

    public async Task AddAsync(ChainExecution execution, CancellationToken ct = default)
    {
        await context.ChainExecutions.AddAsync(execution, ct);
    }

    public Task UpdateAsync(ChainExecution execution, CancellationToken ct = default)
    {
        context.ChainExecutions.Update(execution);
        return Task.CompletedTask;
    }

    public async Task AddEntityMappingAsync(ChainEntityMapping mapping, CancellationToken ct = default)
    {
        await context.ChainEntityMappings.AddAsync(mapping, ct);
    }

    public async Task<int> GetExecutionCountAsync(ChainDefinitionId definitionId, CancellationToken ct = default)
    {
        return await context.ChainExecutions.CountAsync(e => e.ChainDefinitionId == definitionId, ct);
    }

    public async Task<DateTime?> GetLastExecutedAtAsync(ChainDefinitionId definitionId, CancellationToken ct = default)
    {
        return await context.ChainExecutions
            .Where(e => e.ChainDefinitionId == definitionId)
            .OrderByDescending(e => e.StartedAt)
            .Select(e => (DateTime?)e.StartedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await context.SaveChangesAsync(ct);
    }
}
