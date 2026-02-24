using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

/// <summary>
/// Represents a server-side zip generation job.
/// Files are collected, zipped to storage, and the user receives a download link.
/// Jobs have a configurable TTL (default 24h) for auto-cleanup.
/// </summary>
public sealed class ZipJob : AggregateRoot<ZipJobId>
{
#pragma warning disable CS8618
    private ZipJob() { }
#pragma warning restore CS8618

    public static ZipJob Create(
        FamilyId familyId,
        UserId initiatedBy,
        List<Guid> fileIds,
        TimeSpan ttl = default)
    {
        if (ttl == default)
            ttl = TimeSpan.FromHours(24);

        var job = new ZipJob
        {
            Id = ZipJobId.New(),
            FamilyId = familyId,
            InitiatedBy = initiatedBy,
            FileIds = fileIds,
            Status = ZipJobStatus.Pending,
            Progress = 0,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(ttl)
        };

        return job;
    }

    public void MarkProcessing()
    {
        Status = ZipJobStatus.Processing;
    }

    public void UpdateProgress(int progress)
    {
        Progress = Math.Clamp(progress, 0, 100);
    }

    public void MarkCompleted(StorageKey zipStorageKey, long zipSize)
    {
        ZipStorageKey = zipStorageKey;
        ZipSize = zipSize;
        Status = ZipJobStatus.Completed;
        Progress = 100;
        CompletedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ZipJobCompletedEvent(
            Id, FileIds.Count, zipSize, FamilyId));
    }

    public void MarkFailed(string? errorMessage = null)
    {
        Status = ZipJobStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }

    public bool IsExpired => ExpiresAt <= DateTime.UtcNow;

    public FamilyId FamilyId { get; private set; }
    public UserId InitiatedBy { get; private set; }
    public List<Guid> FileIds { get; private set; }
    public ZipJobStatus Status { get; private set; }
    public int Progress { get; private set; }
    public StorageKey? ZipStorageKey { get; private set; }
    public long? ZipSize { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
}
