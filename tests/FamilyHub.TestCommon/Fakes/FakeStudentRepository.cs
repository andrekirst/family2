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

    private IEnumerable<Student> All => _seeded.Concat(AddedStudents);

    public Task<Student?> GetByIdAsync(StudentId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(All.FirstOrDefault(s => s.Id == id));

    public Task<bool> ExistsByIdAsync(StudentId id, CancellationToken cancellationToken = default) =>
        Task.FromResult(All.Any(s => s.Id == id));

    public Task<List<Student>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        var students = All.Where(s => s.FamilyId == familyId).ToList();
        return Task.FromResult(students);
    }

    public Task<bool> ExistsByFamilyMemberIdAsync(FamilyMemberId familyMemberId, CancellationToken cancellationToken = default)
    {
        var exists = All.Any(s => s.FamilyMemberId == familyMemberId);
        return Task.FromResult(exists);
    }

    public Task AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        AddedStudents.Add(student);
        return Task.CompletedTask;
    }
}
