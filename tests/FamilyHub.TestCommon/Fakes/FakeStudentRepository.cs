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

    public Task<Student?> GetByIdAsync(StudentId id, CancellationToken ct = default)
    {
        var student = _seeded.Concat(AddedStudents).FirstOrDefault(s => s.Id == id);
        return Task.FromResult(student);
    }

    public Task<List<Student>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default)
    {
        var students = _seeded.Concat(AddedStudents).Where(s => s.FamilyId == familyId).ToList();
        return Task.FromResult(students);
    }

    public Task<bool> ExistsByFamilyMemberIdAsync(FamilyMemberId familyMemberId, CancellationToken ct = default)
    {
        var exists = _seeded.Concat(AddedStudents).Any(s => s.FamilyMemberId == familyMemberId);
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
