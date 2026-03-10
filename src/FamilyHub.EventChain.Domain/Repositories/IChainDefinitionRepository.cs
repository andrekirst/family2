using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.EventChain.Domain.Repositories;

public interface IChainDefinitionRepository : IWriteRepository<ChainDefinition, ChainDefinitionId>
{
    Task<ChainDefinition?> GetByIdWithStepsAsync(ChainDefinitionId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChainDefinition>> GetByFamilyIdAsync(FamilyId familyId, bool? isEnabled = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChainDefinition>> GetEnabledByTriggerEventTypeAsync(string triggerEventType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChainDefinition>> GetTemplatesAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(ChainDefinition definition, CancellationToken cancellationToken = default);
    Task DeleteAsync(ChainDefinition definition, CancellationToken cancellationToken = default);
}
