using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class OrganizationRuleRepository(AppDbContext context) : IOrganizationRuleRepository
{
    public async Task<OrganizationRule?> GetByIdAsync(OrganizationRuleId id, CancellationToken cancellationToken = default)
        => await context.Set<OrganizationRule>().FindAsync([id], cancellationToken: cancellationToken);

    public async Task<bool> ExistsByIdAsync(OrganizationRuleId id, CancellationToken cancellationToken = default)
        => await context.Set<OrganizationRule>().AnyAsync(r => r.Id == id, cancellationToken);

    public async Task<List<OrganizationRule>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
        => await context.Set<OrganizationRule>()
            .Where(r => r.FamilyId == familyId)
            .OrderBy(r => r.Priority)
            .ToListAsync(cancellationToken);

    public async Task<List<OrganizationRule>> GetEnabledByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
        => await context.Set<OrganizationRule>()
            .Where(r => r.FamilyId == familyId && r.IsEnabled)
            .OrderBy(r => r.Priority)
            .ToListAsync(cancellationToken);

    public async Task<int> GetMaxPriorityAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        var maxPriority = await context.Set<OrganizationRule>()
            .Where(r => r.FamilyId == familyId)
            .MaxAsync(r => (int?)r.Priority, cancellationToken);
        return maxPriority ?? 0;
    }

    public async Task AddAsync(OrganizationRule rule, CancellationToken cancellationToken = default)
        => await context.Set<OrganizationRule>().AddAsync(rule, cancellationToken);

    public Task RemoveAsync(OrganizationRule rule, CancellationToken cancellationToken = default)
    {
        context.Set<OrganizationRule>().Remove(rule);
        return Task.CompletedTask;
    }
}
