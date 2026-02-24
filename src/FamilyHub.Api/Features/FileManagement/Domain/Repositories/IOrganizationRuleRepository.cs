using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IOrganizationRuleRepository
{
    Task<OrganizationRule?> GetByIdAsync(OrganizationRuleId id, CancellationToken ct = default);
    Task<List<OrganizationRule>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default);
    Task<List<OrganizationRule>> GetEnabledByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default);
    Task<int> GetMaxPriorityAsync(FamilyId familyId, CancellationToken ct = default);
    Task AddAsync(OrganizationRule rule, CancellationToken ct = default);
    Task RemoveAsync(OrganizationRule rule, CancellationToken ct = default);
}
