namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;

/// <summary>
/// Configuration for MinIO S3-compatible object storage.
/// Bound from section: FileManagement:Storage:MinIO
/// </summary>
public sealed class MinioStorageOptions
{
    public const string SectionName = "FileManagement:Storage:MinIO";

    public string Endpoint { get; set; } = "localhost:9000";
    public string AccessKey { get; set; } = "minioadmin";
    public string SecretKey { get; set; } = "minioadmin";
    public string BucketName { get; set; } = "familyhub-files";
    public bool UseSSL { get; set; }
}
