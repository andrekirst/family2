using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Domain.Repositories;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(StudentId id, CancellationToken ct = default);

    Task<List<Student>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default);

    Task<bool> ExistsByFamilyMemberIdAsync(FamilyMemberId familyMemberId, CancellationToken ct = default);

    Task AddAsync(Student student, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
