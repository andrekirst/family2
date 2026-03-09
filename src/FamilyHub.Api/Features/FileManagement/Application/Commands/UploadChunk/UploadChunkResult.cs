namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UploadChunk;

public sealed record UploadChunkResult(string UploadId, int ChunkIndex, long Size);
