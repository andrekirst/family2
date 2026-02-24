using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeOrganizationRuleRepository : IOrganizationRuleRepository
{
    public List<OrganizationRule> Rules { get; } = [];

    public Task<OrganizationRule?> GetByIdAsync(OrganizationRuleId id, CancellationToken ct = default)
        => Task.FromResult(Rules.FirstOrDefault(r => r.Id == id));

    public Task<List<OrganizationRule>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default)
        => Task.FromResult(Rules
            .Where(r => r.FamilyId == familyId)
            .OrderBy(r => r.Priority)
            .ToList());

    public Task<List<OrganizationRule>> GetEnabledByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default)
        => Task.FromResult(Rules
            .Where(r => r.FamilyId == familyId && r.IsEnabled)
            .OrderBy(r => r.Priority)
            .ToList());

    public Task<int> GetMaxPriorityAsync(FamilyId familyId, CancellationToken ct = default)
    {
        var max = Rules
            .Where(r => r.FamilyId == familyId)
            .Select(r => (int?)r.Priority)
            .Max() ?? 0;
        return Task.FromResult(max);
    }

    public Task AddAsync(OrganizationRule rule, CancellationToken ct = default)
    {
        Rules.Add(rule);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(OrganizationRule rule, CancellationToken ct = default)
    {
        Rules.Remove(rule);
        return Task.CompletedTask;
    }
}
