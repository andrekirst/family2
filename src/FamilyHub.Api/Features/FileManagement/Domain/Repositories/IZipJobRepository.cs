using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Repositories;

public interface IZipJobRepository
{
    Task<ZipJob?> GetByIdAsync(ZipJobId id, CancellationToken ct = default);
    Task<List<ZipJob>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default);
    Task<int> GetActiveJobCountAsync(FamilyId familyId, CancellationToken ct = default);
    Task<List<ZipJob>> GetExpiredJobsAsync(CancellationToken ct = default);
    Task AddAsync(ZipJob job, CancellationToken ct = default);
    Task RemoveAsync(ZipJob job, CancellationToken ct = default);
}
