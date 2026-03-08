using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;

/// <summary>
/// MinIO (S3-compatible) storage provider.
/// Stores binary data as objects in a configurable bucket.
/// </summary>
public sealed class MinioStorageProvider(IMinioClient minioClient, IOptions<MinioStorageOptions> options)
    : IStorageProvider
{
    private readonly string _bucket = options.Value.BucketName;

    public async Task<string> UploadAsync(Stream data, string mimeType, CancellationToken cancellationToken = default)
    {
        var storageKey = Guid.NewGuid().ToString();

        var args = new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(storageKey)
            .WithStreamData(data)
            .WithObjectSize(data.Length)
            .WithContentType(mimeType);

        await minioClient.PutObjectAsync(args, cancellationToken);

        return storageKey;
    }

    public async Task<Stream?> DownloadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        if (!await ExistsAsync(storageKey, cancellationToken))
            return null;

        var ms = new MemoryStream();

        var args = new GetObjectArgs()
            .WithBucket(_bucket)
            .WithObject(storageKey)
            .WithCallbackStream(async (stream, cancellationToken) =>
            {
                await stream.CopyToAsync(ms, cancellationToken);
            });

        await minioClient.GetObjectAsync(args, cancellationToken);
        ms.Position = 0;
        return ms;
    }

    public async Task<StorageRangeResult?> DownloadRangeAsync(
        string storageKey, long from, long to, CancellationToken cancellationToken = default)
    {
        var totalSize = await GetSizeAsync(storageKey, cancellationToken);
        if (totalSize is null)
            return null;

        var rangeEnd = Math.Min(to, totalSize.Value - 1);
        var length = rangeEnd - from + 1;

        var ms = new MemoryStream();

        var args = new GetObjectArgs()
            .WithBucket(_bucket)
            .WithObject(storageKey)
            .WithOffsetAndLength(from, length)
            .WithCallbackStream(async (stream, cancellationToken) =>
            {
                await stream.CopyToAsync(ms, cancellationToken);
            });

        await minioClient.GetObjectAsync(args, cancellationToken);
        ms.Position = 0;

        return new StorageRangeResult(ms, from, rangeEnd, totalSize.Value);
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(_bucket)
            .WithObject(storageKey);

        await minioClient.RemoveObjectAsync(args, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var args = new StatObjectArgs()
                .WithBucket(_bucket)
                .WithObject(storageKey);

            await minioClient.StatObjectAsync(args, cancellationToken);
            return true;
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
            return false;
        }
    }

    public async Task<long?> GetSizeAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var args = new StatObjectArgs()
                .WithBucket(_bucket)
                .WithObject(storageKey);

            var stat = await minioClient.StatObjectAsync(args, cancellationToken);
            return stat.Size;
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
            return null;
        }
    }
}
