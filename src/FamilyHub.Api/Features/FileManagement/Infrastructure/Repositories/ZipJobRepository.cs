using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Repositories;

public sealed class ZipJobRepository(AppDbContext context) : IZipJobRepository
{
    public async Task<ZipJob?> GetByIdAsync(ZipJobId id, CancellationToken cancellationToken = default)
        => await context.Set<ZipJob>().FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public async Task<bool> ExistsByIdAsync(ZipJobId id, CancellationToken cancellationToken = default)
        => await context.Set<ZipJob>().AnyAsync(j => j.Id == id, cancellationToken);

    public async Task<List<ZipJob>> GetByFamilyIdAsync(FamilyId familyId, CancellationToken cancellationToken = default)
        => await context.Set<ZipJob>()
            .Where(j => j.FamilyId == familyId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<int> GetActiveJobCountAsync(FamilyId familyId, CancellationToken cancellationToken = default)
        => await context.Set<ZipJob>()
            .CountAsync(j => j.FamilyId == familyId &&
                (j.Status == ZipJobStatus.Pending || j.Status == ZipJobStatus.Processing), cancellationToken);

    public async Task<List<ZipJob>> GetExpiredJobsAsync(CancellationToken cancellationToken = default)
        => await context.Set<ZipJob>()
            .Where(j => j.ExpiresAt <= DateTime.UtcNow && j.Status == ZipJobStatus.Completed)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(ZipJob job, CancellationToken cancellationToken = default)
        => await context.Set<ZipJob>().AddAsync(job, cancellationToken);

    public Task RemoveAsync(ZipJob job, CancellationToken cancellationToken = default)
    {
        context.Set<ZipJob>().Remove(job);
        return Task.CompletedTask;
    }
}
