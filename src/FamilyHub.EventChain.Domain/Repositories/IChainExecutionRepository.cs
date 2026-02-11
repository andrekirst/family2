using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Enums;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.EventChain.Domain.Repositories;

public interface IChainExecutionRepository
{
    Task<ChainExecution?> GetByIdAsync(ChainExecutionId id, CancellationToken ct = default);
    Task<ChainExecution?> GetByIdWithStepsAsync(ChainExecutionId id, CancellationToken ct = default);
    Task<IReadOnlyList<ChainExecution>> GetByFamilyIdAsync(FamilyId familyId, ChainDefinitionId? chainDefinitionId = null, ChainExecutionStatus? status = null, CancellationToken ct = default);
    Task<StepExecution?> GetStepExecutionAsync(Guid stepExecutionId, CancellationToken ct = default);
    Task<IReadOnlyList<ChainEntityMapping>> GetEntityMappingsAsync(Guid entityId, string? entityType = null, CancellationToken ct = default);
    Task AddAsync(ChainExecution execution, CancellationToken ct = default);
    Task UpdateAsync(ChainExecution execution, CancellationToken ct = default);
    Task AddEntityMappingAsync(ChainEntityMapping mapping, CancellationToken ct = default);
    Task<int> GetExecutionCountAsync(ChainDefinitionId definitionId, CancellationToken ct = default);
    Task<DateTime?> GetLastExecutedAtAsync(ChainDefinitionId definitionId, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
