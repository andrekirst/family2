using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.TestCommon.Fakes;

public class FakeFileManagementStorageService : IFileManagementStorageService
{
    public List<string> DeletedStorageKeys { get; } = [];

    public Task<FileStorageResult> StoreFileAsync(FamilyId familyId, Stream data, string fileName, CancellationToken ct = default)
    {
        var result = new FileStorageResult(
            $"fake-key-{Guid.NewGuid()}",
            MimeType.From("application/octet-stream"),
            FileSize.From(data.Length),
            Checksum.From("fake-checksum"));
        return Task.FromResult(result);
    }

    public Task<FileDownloadResult?> GetFileAsync(string storageKey, CancellationToken ct = default)
        => Task.FromResult<FileDownloadResult?>(null);

    public Task<StorageRangeResult?> GetFileRangeAsync(string storageKey, long from, long to, CancellationToken ct = default)
        => Task.FromResult<StorageRangeResult?>(null);

    public Task DeleteFileAsync(FamilyId familyId, string storageKey, long fileSize, CancellationToken ct = default)
    {
        DeletedStorageKeys.Add(storageKey);
        return Task.CompletedTask;
    }

    public Task<string> InitiateChunkedUploadAsync(CancellationToken ct = default)
        => Task.FromResult($"upload-{Guid.NewGuid()}");

    public Task UploadChunkAsync(string uploadId, int chunkIndex, Stream data, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<FileStorageResult> CompleteChunkedUploadAsync(FamilyId familyId, string uploadId, string fileName, CancellationToken ct = default)
    {
        var result = new FileStorageResult(
            $"fake-key-{Guid.NewGuid()}",
            MimeType.From("application/octet-stream"),
            FileSize.From(0),
            Checksum.From("fake-checksum"));
        return Task.FromResult(result);
    }
}
