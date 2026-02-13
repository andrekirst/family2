namespace FamilyHub.Api.Common.Infrastructure.Avatar;

/// <summary>
/// Abstraction for binary file storage.
/// Phase 1: PostgreSQL large objects. Future: S3/Azure Blob Storage.
/// </summary>
public interface IFileStorageService
{
    Task<string> SaveAsync(byte[] data, string mimeType, CancellationToken ct = default);
    Task<byte[]?> GetAsync(string storageKey, CancellationToken ct = default);
    Task DeleteAsync(string storageKey, CancellationToken ct = default);
}
