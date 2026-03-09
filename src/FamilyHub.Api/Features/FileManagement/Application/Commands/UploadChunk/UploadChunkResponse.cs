namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UploadChunk;

/// <summary>
/// REST response DTO for the upload chunk endpoint.
/// </summary>
public sealed record UploadChunkResponse(string UploadId, int ChunkIndex, long Size);
