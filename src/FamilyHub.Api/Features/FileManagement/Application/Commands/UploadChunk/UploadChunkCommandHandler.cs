using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UploadChunk;

/// <summary>
/// Records chunk metadata after the endpoint has already uploaded the bytes via IFileManagementStorageService.
/// The pipeline provides validation, logging, user resolution, and audit trail automatically.
/// </summary>
public sealed class UploadChunkCommandHandler
    : ICommandHandler<UploadChunkCommand, Result<UploadChunkResult>>
{
    public ValueTask<Result<UploadChunkResult>> Handle(
        UploadChunkCommand command,
        CancellationToken cancellationToken)
    {
        var result = new UploadChunkResult(
            command.UploadId,
            command.ChunkIndex,
            command.ChunkSize);

        return new ValueTask<Result<UploadChunkResult>>(result);
    }
}
