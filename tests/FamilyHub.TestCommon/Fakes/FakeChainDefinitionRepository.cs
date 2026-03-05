using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.EventChain.Domain.Entities;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeChainDefinitionRepository(List<ChainDefinition>? existingDefinitions = null) : IChainDefinitionRepository
{
    private readonly List<ChainDefinition> _definitions = existingDefinitions ?? [];
    public List<ChainDefinition> AddedDefinitions { get; } = [];

    public Task<ChainDefinition?> GetByIdAsync(ChainDefinitionId id, CancellationToken ct = default) =>
        Task.FromResult(_definitions.Concat(AddedDefinitions).FirstOrDefault(d => d.Id == id));

    public Task<ChainDefinition?> GetByIdWithStepsAsync(ChainDefinitionId id, CancellationToken ct = default) =>
        GetByIdAsync(id, ct);

    public Task<IReadOnlyList<ChainDefinition>> GetByFamilyIdAsync(
        FamilyId familyId, bool? isEnabled = null, CancellationToken ct = default)
    {
        var query = _definitions.Concat(AddedDefinitions)
            .Where(d => d.FamilyId == familyId);

        if (isEnabled.HasValue)
            query = query.Where(d => d.IsEnabled == isEnabled.Value);

        return Task.FromResult<IReadOnlyList<ChainDefinition>>(query.ToList().AsReadOnly());
    }

    public Task<IReadOnlyList<ChainDefinition>> GetEnabledByTriggerEventTypeAsync(
        string triggerEventType, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ChainDefinition>>(_definitions.Concat(AddedDefinitions)
            .Where(d => d.IsEnabled && d.TriggerEventType == triggerEventType)
            .ToList().AsReadOnly());

    public Task<IReadOnlyList<ChainDefinition>> GetTemplatesAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ChainDefinition>>(_definitions.Concat(AddedDefinitions)
            .Where(d => d.IsTemplate)
            .ToList().AsReadOnly());

    public Task AddAsync(ChainDefinition definition, CancellationToken ct = default)
    {
        AddedDefinitions.Add(definition);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ChainDefinition definition, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task DeleteAsync(ChainDefinition definition, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        Task.FromResult(1);
}
