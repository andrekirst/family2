using FamilyHub.Api.Common.Database;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.School.Infrastructure.Repositories;

public sealed class SchoolRepository(AppDbContext context) : ISchoolRepository
{
    public async Task<Domain.Entities.School?> GetByIdAsync(SchoolId id, CancellationToken cancellationToken = default)
    {
        return await context.Schools.FindAsync([id], cancellationToken: cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(SchoolId id, CancellationToken cancellationToken = default)
    {
        return await context.Schools.AnyAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<List<Domain.Entities.School>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        return await context.Schools
            .Where(s => s.FamilyId == familyId)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Domain.Entities.School school, CancellationToken cancellationToken = default)
    {
        await context.Schools.AddAsync(school, cancellationToken);
    }

    public Task UpdateAsync(Domain.Entities.School school, CancellationToken cancellationToken = default)
    {
        context.Schools.Update(school);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Domain.Entities.School school, CancellationToken cancellationToken = default)
    {
        context.Schools.Remove(school);
        return Task.CompletedTask;
    }
}
