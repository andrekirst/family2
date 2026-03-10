using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Enums;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.EventChain.Domain.Repositories;

public interface IChainExecutionRepository : IWriteRepository<ChainExecution, ChainExecutionId>
{
    Task<ChainExecution?> GetByIdWithStepsAsync(ChainExecutionId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChainExecution>> GetByFamilyIdAsync(FamilyId familyId, ChainDefinitionId? chainDefinitionId = null, ChainExecutionStatus? status = null, CancellationToken cancellationToken = default);
    Task<StepExecution?> GetStepExecutionAsync(Guid stepExecutionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChainEntityMapping>> GetEntityMappingsAsync(Guid entityId, string? entityType = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(ChainExecution execution, CancellationToken cancellationToken = default);
    Task AddEntityMappingAsync(ChainEntityMapping mapping, CancellationToken cancellationToken = default);
    Task<int> GetExecutionCountAsync(ChainDefinitionId definitionId, CancellationToken cancellationToken = default);
    Task<DateTime?> GetLastExecutedAtAsync(ChainDefinitionId definitionId, CancellationToken cancellationToken = default);
}
