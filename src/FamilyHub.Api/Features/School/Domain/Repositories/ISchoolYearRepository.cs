using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Domain.Repositories;

public interface ISchoolYearRepository : IWriteRepository<SchoolYear, SchoolYearId>
{
    Task<List<SchoolYear>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default);

    Task UpdateAsync(SchoolYear schoolYear, CancellationToken cancellationToken = default);

    Task DeleteAsync(SchoolYear schoolYear, CancellationToken cancellationToken = default);
}
