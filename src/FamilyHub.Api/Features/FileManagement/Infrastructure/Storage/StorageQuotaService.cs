using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Data;
using FamilyHub.Common.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;

/// <summary>
/// Manages per-family storage quotas: tracking usage, checking limits, updating on upload/delete.
/// </summary>
public interface IStorageQuotaService
{
    Task<StorageQuotaInfo> GetQuotaAsync(FamilyId familyId, CancellationToken cancellationToken = default);
    Task<bool> CanUploadAsync(FamilyId familyId, long fileSizeBytes, CancellationToken cancellationToken = default);
    Task IncrementUsageAsync(FamilyId familyId, long bytes, CancellationToken cancellationToken = default);
    Task DecrementUsageAsync(FamilyId familyId, long bytes, CancellationToken cancellationToken = default);
}

/// <summary>
/// Quota information for a family.
/// </summary>
public sealed record StorageQuotaInfo(long UsedBytes, long MaxBytes)
{
    public long RemainingBytes => MaxBytes - UsedBytes;
    public double UsagePercentage => MaxBytes > 0 ? (double)UsedBytes / MaxBytes * 100 : 0;
    public bool IsExceeded => UsedBytes >= MaxBytes;
}

public sealed class StorageQuotaService(
    AppDbContext dbContext,
    TimeProvider timeProvider,
    IOptions<StorageQuotaOptions> options) : IStorageQuotaService
{
    private readonly StorageQuotaOptions _options = options.Value;

    public async Task<StorageQuotaInfo> GetQuotaAsync(FamilyId familyId, CancellationToken cancellationToken = default)
    {
        var quota = await GetOrCreateQuotaAsync(familyId, cancellationToken);
        return new StorageQuotaInfo(quota.UsedBytes, quota.MaxBytes);
    }

    public async Task<bool> CanUploadAsync(FamilyId familyId, long fileSizeBytes, CancellationToken cancellationToken = default)
    {
        var quota = await GetOrCreateQuotaAsync(familyId, cancellationToken);
        return quota.UsedBytes + fileSizeBytes <= quota.MaxBytes;
    }

    public async Task IncrementUsageAsync(FamilyId familyId, long bytes, CancellationToken cancellationToken = default)
    {
        var quota = await GetOrCreateQuotaAsync(familyId, cancellationToken);
        quota.UsedBytes += bytes;
        quota.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
    }

    public async Task DecrementUsageAsync(FamilyId familyId, long bytes, CancellationToken cancellationToken = default)
    {
        var quota = await GetOrCreateQuotaAsync(familyId, cancellationToken);
        quota.UsedBytes = Math.Max(0, quota.UsedBytes - bytes);
        quota.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
    }

    private async Task<StorageQuota> GetOrCreateQuotaAsync(FamilyId familyId, CancellationToken cancellationToken)
    {
        // Check local change tracker first — the entity may have been added
        // in a prior call within the same scope but not yet saved to the database.
        var quota = dbContext.Set<StorageQuota>().Local
            .FirstOrDefault(q => q.FamilyId == familyId.Value);

        quota ??= await dbContext.Set<StorageQuota>()
            .FirstOrDefaultAsync(q => q.FamilyId == familyId.Value, cancellationToken);

        if (quota is null)
        {
            quota = new StorageQuota
            {
                FamilyId = familyId.Value,
                UsedBytes = 0,
                MaxBytes = _options.DefaultQuotaBytes,
                UpdatedAt = timeProvider.GetUtcNow().UtcDateTime
            };
            dbContext.Set<StorageQuota>().Add(quota);
        }

        return quota;
    }
}
