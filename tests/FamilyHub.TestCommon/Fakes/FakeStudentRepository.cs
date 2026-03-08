using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeStudentRepository(List<Student>? seededStudents = null) : IStudentRepository
{
    private readonly List<Student> _seeded = seededStudents ?? [];
    public List<Student> AddedStudents { get; } = [];

    private IEnumerable<Student> All => All;

    public Task<Student?> GetByIdAsync(StudentId id, CancellationToken ct = default) =>
        Task.FromResult(All.FirstOrDefault(s => s.Id == id));

    public Task<bool> ExistsByIdAsync(StudentId id, CancellationToken ct = default) =>
        Task.FromResult(All.Any(s => s.Id == id));

    public Task<List<Student>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default)
    {
        var students = All.Where(s => s.FamilyId == familyId).ToList();
        return Task.FromResult(students);
    }

    public Task<bool> ExistsByFamilyMemberIdAsync(FamilyMemberId familyMemberId, CancellationToken ct = default)
    {
        var exists = All.Any(s => s.FamilyMemberId == familyMemberId);
        return Task.FromResult(exists);
    }

    public Task AddAsync(Student student, CancellationToken ct = default)
    {
        AddedStudents.Add(student);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        Task.FromResult(1);
}
