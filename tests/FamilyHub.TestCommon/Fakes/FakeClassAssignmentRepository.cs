using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeClassAssignmentRepository(List<ClassAssignment>? seededAssignments = null) : IClassAssignmentRepository
{
    private readonly List<ClassAssignment> _seeded = seededAssignments ?? [];
    public List<ClassAssignment> AddedAssignments { get; } = [];
    public List<ClassAssignment> UpdatedAssignments { get; } = [];
    public List<ClassAssignment> DeletedAssignments { get; } = [];

    private IEnumerable<ClassAssignment> All => _seeded.Concat(AddedAssignments);

    public Task<ClassAssignment?> GetByIdAsync(ClassAssignmentId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(All.FirstOrDefault(a => a.Id == id));

    public Task<bool> ExistsByIdAsync(ClassAssignmentId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(All.Any(a => a.Id == id));

    public Task<List<ClassAssignment>> GetByStudentIdAsync(StudentId studentId, CancellationToken cancellationToken = default)
    {
        var assignments = All.Where(a => a.StudentId == studentId).ToList();
        return Task.FromResult(assignments);
    }

    public Task<List<ClassAssignment>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        var assignments = All.Where(a => a.FamilyId == familyId).ToList();
        return Task.FromResult(assignments);
    }

    public Task<bool> ExistsBySchoolIdAsync(SchoolId schoolId, CancellationToken cancellationToken = default) =>
        Task.FromResult(All.Any(a => a.SchoolId == schoolId));

    public Task<bool> ExistsBySchoolYearIdAsync(SchoolYearId schoolYearId, CancellationToken cancellationToken = default) =>
        Task.FromResult(All.Any(a => a.SchoolYearId == schoolYearId));

    public Task AddAsync(ClassAssignment assignment, CancellationToken cancellationToken = default)
    {
        AddedAssignments.Add(assignment);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ClassAssignment classAssignment, CancellationToken cancellationToken = default)
    {
        UpdatedAssignments.Add(classAssignment);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ClassAssignment assignment, CancellationToken cancellationToken = default)
    {
        DeletedAssignments.Add(assignment);
        _seeded.Remove(assignment);
        return Task.CompletedTask;
    }
}
