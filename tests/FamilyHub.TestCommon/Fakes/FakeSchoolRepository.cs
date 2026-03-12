using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using SchoolEntity = FamilyHub.Api.Features.School.Domain.Entities.School;

namespace FamilyHub.TestCommon.Fakes;

public class FakeSchoolRepository(List<SchoolEntity>? seededSchools = null) : ISchoolRepository
{
    private readonly List<SchoolEntity> _seeded = seededSchools ?? [];
    public List<SchoolEntity> AddedSchools { get; } = [];
    public List<SchoolEntity> UpdatedSchools { get; } = [];
    public List<SchoolEntity> DeletedSchools { get; } = [];

    private IEnumerable<SchoolEntity> All => _seeded.Concat(AddedSchools);

    public Task<SchoolEntity?> GetByIdAsync(SchoolId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(All.FirstOrDefault(s => s.Id == id));

    public Task<bool> ExistsByIdAsync(SchoolId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(All.Any(s => s.Id == id));

    public Task<List<SchoolEntity>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        var schools = All.Where(s => s.FamilyId == familyId).ToList();
        return Task.FromResult(schools);
    }

    public Task AddAsync(SchoolEntity school, CancellationToken cancellationToken = default)
    {
        AddedSchools.Add(school);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(SchoolEntity school, CancellationToken cancellationToken = default)
    {
        UpdatedSchools.Add(school);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(SchoolEntity school, CancellationToken cancellationToken = default)
    {
        DeletedSchools.Add(school);
        _seeded.Remove(school);
        return Task.CompletedTask;
    }
}
