namespace FamilyHub.Api.Features.FileManagement.Infrastructure.Data;

/// <summary>
/// Temporary storage for chunked upload parts.
/// Chunks are assembled into a complete file after all parts arrive.
/// </summary>
public sealed class UploadChunk
{
    public Guid Id { get; set; }
    public string UploadId { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public byte[] Data { get; set; } = [];
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
}
