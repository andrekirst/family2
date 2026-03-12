using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeSchoolYearRepository(List<SchoolYear>? seededSchoolYears = null) : ISchoolYearRepository
{
    private readonly List<SchoolYear> _seeded = seededSchoolYears ?? [];
    public List<SchoolYear> AddedSchoolYears { get; } = [];
    public List<SchoolYear> UpdatedSchoolYears { get; } = [];
    public List<SchoolYear> DeletedSchoolYears { get; } = [];

    private IEnumerable<SchoolYear> All => _seeded.Concat(AddedSchoolYears);

    public Task<SchoolYear?> GetByIdAsync(SchoolYearId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(All.FirstOrDefault(sy => sy.Id == id));

    public Task<bool> ExistsByIdAsync(SchoolYearId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(All.Any(sy => sy.Id == id));

    public Task<List<SchoolYear>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        var schoolYears = All.Where(sy => sy.FamilyId == familyId).ToList();
        return Task.FromResult(schoolYears);
    }

    public Task AddAsync(SchoolYear schoolYear, CancellationToken cancellationToken = default)
    {
        AddedSchoolYears.Add(schoolYear);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(SchoolYear schoolYear, CancellationToken cancellationToken = default)
    {
        UpdatedSchoolYears.Add(schoolYear);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(SchoolYear schoolYear, CancellationToken cancellationToken = default)
    {
        DeletedSchoolYears.Add(schoolYear);
        _seeded.Remove(schoolYear);
        return Task.CompletedTask;
    }
}
