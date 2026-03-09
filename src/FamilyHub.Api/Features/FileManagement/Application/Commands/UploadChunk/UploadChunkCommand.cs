using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UploadChunk;

/// <summary>
/// Records chunk upload metadata after the endpoint has already uploaded bytes via IFileManagementStorageService.
/// SECURITY FIX: Adds IRequireFamily to enforce family membership check (previously missing).
/// </summary>
public sealed record UploadChunkCommand(
    string UploadId,
    int ChunkIndex,
    long ChunkSize
) : ICommand<Result<UploadChunkResult>>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
