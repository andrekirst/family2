using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.InitiateChunkedUpload;

/// <summary>
/// Initiates a chunked upload session via the storage service.
/// The pipeline provides validation, logging, user resolution, and audit trail automatically.
/// </summary>
public sealed class InitiateChunkedUploadCommandHandler(
    IFileManagementStorageService storageService)
    : ICommandHandler<InitiateChunkedUploadCommand, Result<InitiateChunkedUploadResult>>
{
    public async ValueTask<Result<InitiateChunkedUploadResult>> Handle(
        InitiateChunkedUploadCommand command,
        CancellationToken cancellationToken)
    {
        var uploadId = await storageService.InitiateChunkedUploadAsync(cancellationToken);

        return new InitiateChunkedUploadResult(uploadId);
    }
}
