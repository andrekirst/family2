using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeZipJobRepository : IZipJobRepository
{
    public List<ZipJob> Jobs { get; } = [];

    public Task<ZipJob?> GetByIdAsync(ZipJobId id, CancellationToken ct = default)
        => Task.FromResult(Jobs.FirstOrDefault(j => j.Id == id));

    public Task<List<ZipJob>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken ct = default)
        => Task.FromResult(Jobs
            .Where(j => j.FamilyId == familyId)
            .OrderByDescending(j => j.CreatedAt)
            .ToList());

    public Task<int> GetActiveJobCountAsync(FamilyId familyId, CancellationToken ct = default)
        => Task.FromResult(Jobs.Count(j =>
            j.FamilyId == familyId &&
            (j.Status == ZipJobStatus.Pending || j.Status == ZipJobStatus.Processing)));

    public Task<List<ZipJob>> GetExpiredJobsAsync(CancellationToken ct = default)
        => Task.FromResult(Jobs
            .Where(j => j.ExpiresAt <= DateTime.UtcNow && j.Status == ZipJobStatus.Completed)
            .ToList());

    public Task AddAsync(ZipJob job, CancellationToken ct = default)
    {
        Jobs.Add(job);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(ZipJob job, CancellationToken ct = default)
    {
        Jobs.Remove(job);
        return Task.CompletedTask;
    }
}
