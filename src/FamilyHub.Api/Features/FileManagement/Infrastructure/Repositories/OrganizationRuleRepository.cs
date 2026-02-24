using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class OrganizationRuleRepository(AppDbContext context) : IOrganizationRuleRepository
{
    public async Task<OrganizationRule?> GetByIdAsync(OrganizationRuleId id, CancellationToken ct = default)
        => await context.Set<OrganizationRule>().FindAsync([id], cancellationToken: ct);

    public async Task<List<OrganizationRule>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default)
        => await context.Set<OrganizationRule>()
            .Where(r => r.FamilyId == familyId)
            .OrderBy(r => r.Priority)
            .ToListAsync(ct);

    public async Task<List<OrganizationRule>> GetEnabledByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default)
        => await context.Set<OrganizationRule>()
            .Where(r => r.FamilyId == familyId && r.IsEnabled)
            .OrderBy(r => r.Priority)
            .ToListAsync(ct);

    public async Task<int> GetMaxPriorityAsync(FamilyId familyId, CancellationToken ct = default)
    {
        var maxPriority = await context.Set<OrganizationRule>()
            .Where(r => r.FamilyId == familyId)
            .MaxAsync(r => (int?)r.Priority, ct);
        return maxPriority ?? 0;
    }

    public async Task AddAsync(OrganizationRule rule, CancellationToken ct = default)
        => await context.Set<OrganizationRule>().AddAsync(rule, ct);

    public Task RemoveAsync(OrganizationRule rule, CancellationToken ct = default)
    {
        context.Set<OrganizationRule>().Remove(rule);
        return Task.CompletedTask;
    }
}
