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
    Task<StorageQuotaInfo> GetQuotaAsync(FamilyId familyId, CancellationToken ct = default);
    Task<bool> CanUploadAsync(FamilyId familyId, long fileSizeBytes, CancellationToken ct = default);
    Task IncrementUsageAsync(FamilyId familyId, long bytes, CancellationToken ct = default);
    Task DecrementUsageAsync(FamilyId familyId, long bytes, CancellationToken ct = default);
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
    IOptions<StorageQuotaOptions> options) : IStorageQuotaService
{
    private readonly StorageQuotaOptions _options = options.Value;

    public async Task<StorageQuotaInfo> GetQuotaAsync(FamilyId familyId, CancellationToken ct = default)
    {
        var quota = await GetOrCreateQuotaAsync(familyId, ct);
        return new StorageQuotaInfo(quota.UsedBytes, quota.MaxBytes);
    }

    public async Task<bool> CanUploadAsync(FamilyId familyId, long fileSizeBytes, CancellationToken ct = default)
    {
        var quota = await GetOrCreateQuotaAsync(familyId, ct);
        return quota.UsedBytes + fileSizeBytes <= quota.MaxBytes;
    }

    public async Task IncrementUsageAsync(FamilyId familyId, long bytes, CancellationToken ct = default)
    {
        var quota = await GetOrCreateQuotaAsync(familyId, ct);
        quota.UsedBytes += bytes;
        quota.UpdatedAt = DateTime.UtcNow;
    }

    public async Task DecrementUsageAsync(FamilyId familyId, long bytes, CancellationToken ct = default)
    {
        var quota = await GetOrCreateQuotaAsync(familyId, ct);
        quota.UsedBytes = Math.Max(0, quota.UsedBytes - bytes);
        quota.UpdatedAt = DateTime.UtcNow;
    }

    private async Task<StorageQuota> GetOrCreateQuotaAsync(FamilyId familyId, CancellationToken ct)
    {
        var quota = await dbContext.Set<StorageQuota>()
            .FirstOrDefaultAsync(q => q.FamilyId == familyId.Value, ct);

        if (quota is null)
        {
            quota = new StorageQuota
            {
                FamilyId = familyId.Value,
                UsedBytes = 0,
                MaxBytes = _options.DefaultQuotaBytes,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Set<StorageQuota>().Add(quota);
        }

        return quota;
    }
}
