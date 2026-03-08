using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Domain.Repositories;

public interface IStudentRepository : IWriteRepository<Student, StudentId>
{
    Task<List<Student>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default);

    Task<bool> ExistsByFamilyMemberIdAsync(FamilyMemberId familyMemberId, CancellationToken ct = default);

}
