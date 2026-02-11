using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.EventChain.Domain.Repositories;

public interface IChainDefinitionRepository
{
    Task<ChainDefinition?> GetByIdAsync(ChainDefinitionId id, CancellationToken ct = default);
    Task<ChainDefinition?> GetByIdWithStepsAsync(ChainDefinitionId id, CancellationToken ct = default);
    Task<IReadOnlyList<ChainDefinition>> GetByFamilyIdAsync(FamilyId familyId, bool? isEnabled = null, CancellationToken ct = default);
    Task<IReadOnlyList<ChainDefinition>> GetEnabledByTriggerEventTypeAsync(string triggerEventType, CancellationToken ct = default);
    Task<IReadOnlyList<ChainDefinition>> GetTemplatesAsync(CancellationToken ct = default);
    Task AddAsync(ChainDefinition definition, CancellationToken ct = default);
    Task UpdateAsync(ChainDefinition definition, CancellationToken ct = default);
    Task DeleteAsync(ChainDefinition definition, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
