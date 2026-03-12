using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Domain.Repositories;

public interface IClassAssignmentRepository : IWriteRepository<ClassAssignment, ClassAssignmentId>
{
    Task<List<ClassAssignment>> GetByStudentIdAsync(StudentId studentId, CancellationToken cancellationToken = default);

    Task<List<ClassAssignment>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySchoolIdAsync(SchoolId schoolId, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySchoolYearIdAsync(SchoolYearId schoolYearId, CancellationToken cancellationToken = default);

    Task UpdateAsync(ClassAssignment classAssignment, CancellationToken cancellationToken = default);

    Task DeleteAsync(ClassAssignment classAssignment, CancellationToken cancellationToken = default);
}
