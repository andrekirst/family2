using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;

/// <summary>
/// Creates the MinIO bucket on application startup if it doesn't already exist.
/// </summary>
public sealed class MinioBucketInitializer(
    IMinioClient minioClient,
    IOptions<MinioStorageOptions> options,
    ILogger<MinioBucketInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var bucket = options.Value.BucketName;

        try
        {
            var exists = await minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(bucket), cancellationToken);

            if (!exists)
            {
                await minioClient.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(bucket), cancellationToken);
                logger.LogInformation("Created MinIO bucket: {Bucket}", bucket);
            }
            else
            {
                logger.LogInformation("MinIO bucket already exists: {Bucket}", bucket);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize MinIO bucket '{Bucket}'. File uploads will fail until MinIO is available", bucket);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
