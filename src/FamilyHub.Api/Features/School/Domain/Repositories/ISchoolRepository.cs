using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Domain.Entities;
using FamilyHub.Api.Features.School.Domain.ValueObjects;

namespace FamilyHub.Api.Features.School.Domain.Repositories;

public interface ISchoolRepository : IWriteRepository<Entities.School, SchoolId>
{
    Task<List<Entities.School>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default);

    Task UpdateAsync(Entities.School school, CancellationToken cancellationToken = default);

    Task DeleteAsync(Entities.School school, CancellationToken cancellationToken = default);
}
