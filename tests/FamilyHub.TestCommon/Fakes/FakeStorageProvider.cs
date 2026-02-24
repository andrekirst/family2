using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;

namespace FamilyHub.TestCommon.Fakes;

public class FakeStorageProvider : IStorageProvider
{
    private readonly Dictionary<string, byte[]> _storage = new();

    public void SeedFile(string storageKey, byte[] data)
        => _storage[storageKey] = data;

    public Task<string> UploadAsync(Stream data, string mimeType, CancellationToken ct = default)
    {
        var key = $"fake-{Guid.NewGuid()}";
        using var ms = new MemoryStream();
        data.CopyTo(ms);
        _storage[key] = ms.ToArray();
        return Task.FromResult(key);
    }

    public Task<Stream?> DownloadAsync(string storageKey, CancellationToken ct = default)
    {
        if (_storage.TryGetValue(storageKey, out var data))
            return Task.FromResult<Stream?>(new MemoryStream(data));
        return Task.FromResult<Stream?>(null);
    }

    public Task<StorageRangeResult?> DownloadRangeAsync(string storageKey, long from, long to, CancellationToken ct = default)
    {
        if (!_storage.TryGetValue(storageKey, out var data))
            return Task.FromResult<StorageRangeResult?>(null);

        var rangeData = data.Skip((int)from).Take((int)(to - from + 1)).ToArray();
        return Task.FromResult<StorageRangeResult?>(new StorageRangeResult(
            new MemoryStream(rangeData), from, to, data.Length));
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        _storage.Remove(storageKey);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default)
        => Task.FromResult(_storage.ContainsKey(storageKey));

    public Task<long?> GetSizeAsync(string storageKey, CancellationToken ct = default)
    {
        if (_storage.TryGetValue(storageKey, out var data))
            return Task.FromResult<long?>(data.Length);
        return Task.FromResult<long?>(null);
    }
}
