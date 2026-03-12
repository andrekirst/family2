using FamilyHub.Api.Common.Database;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.School.Infrastructure.Repositories;

public sealed class SchoolYearRepository(AppDbContext context) : ISchoolYearRepository
{
    public async Task<SchoolYear?> GetByIdAsync(SchoolYearId id, CancellationToken cancellationToken = default)
    {
        return await context.SchoolYears.FindAsync([id], cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(SchoolYearId id, CancellationToken cancellationToken = default)
    {
        return await context.SchoolYears.AnyAsync(sy => sy.Id == id, cancellationToken);
    }

    public async Task<List<SchoolYear>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await context.SchoolYears
            .Where(sy => sy.FamilyId == familyId)
            .OrderByDescending(sy => sy.StartYear)
            .ThenByDescending(sy => sy.EndYear)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SchoolYear schoolYear, CancellationToken cancellationToken = default)
    {
        await context.SchoolYears.AddAsync(schoolYear, cancellationToken);
    }

    public Task UpdateAsync(SchoolYear schoolYear, CancellationToken cancellationToken = default)
    {
        context.SchoolYears.Update(schoolYear);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(SchoolYear schoolYear, CancellationToken cancellationToken = default)
    {
        context.SchoolYears.Remove(schoolYear);
        return Task.CompletedTask;
    }
}
