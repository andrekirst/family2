using FamilyHub.Api.Common.Infrastructure.Avatar;

namespace FamilyHub.TestCommon.Fakes;

public class FakeFileStorageService : IFileStorageService
{
    public Dictionary<string, byte[]> StoredFiles { get; } = [];
    public List<string> DeletedKeys { get; } = [];
    private int _keyCounter;

    public Task<string> SaveAsync(byte[] data, string mimeType, CancellationToken ct = default)
    {
        var key = $"fake-storage-key-{Interlocked.Increment(ref _keyCounter)}";
        StoredFiles[key] = data;
        return Task.FromResult(key);
    }

    public Task<byte[]?> GetAsync(string storageKey, CancellationToken ct = default) =>
        Task.FromResult(StoredFiles.TryGetValue(storageKey, out var data) ? data : null);

    public Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        DeletedKeys.Add(storageKey);
        StoredFiles.Remove(storageKey);
        return Task.CompletedTask;
    }
}
