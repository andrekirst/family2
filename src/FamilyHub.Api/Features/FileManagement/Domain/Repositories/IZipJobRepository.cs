using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IZipJobRepository : IWriteRepository<ZipJob, ZipJobId>
{
    Task<List<ZipJob>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default);
    Task<int> GetActiveJobCountAsync(FamilyId familyId, CancellationToken cancellationToken = default);
    Task<List<ZipJob>> GetExpiredJobsAsync(CancellationToken cancellationToken = default);
    Task RemoveAsync(ZipJob job, CancellationToken cancellationToken = default);
}
