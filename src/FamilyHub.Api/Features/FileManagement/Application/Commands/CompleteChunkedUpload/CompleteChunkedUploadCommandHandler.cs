using FamilyHub.Api.Common.Infrastructure.BlobStaging;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CompleteChunkedUpload;

/// <summary>
/// Completes a chunked upload by assembling all chunks via the storage service.
/// The pipeline provides validation, logging, user resolution, and audit trail automatically.
/// A blob staging entry is written in the same transaction for the outbox pattern.
/// </summary>
public sealed class CompleteChunkedUploadCommandHandler(
    IFileManagementStorageService storageService,
    IBlobStagingRepository blobStagingRepository)
    : ICommandHandler<CompleteChunkedUploadCommand, Result<CompleteChunkedUploadResult>>
{
    public async ValueTask<Result<CompleteChunkedUploadResult>> Handle(
        CompleteChunkedUploadCommand command,
        CancellationToken cancellationToken)
    {
        var storageResult = await storageService.CompleteChunkedUploadAsync(
            command.FamilyId, command.UploadId, command.FileName, cancellationToken);

        // Write blob staging entry in same transaction for outbox pattern
        await blobStagingRepository.AddAsync(new BlobStagingEntry
        {
            Module = "FileManagement",
            StorageKey = storageResult.StorageKey,
            MaxRetries = 5
        }, cancellationToken);

        return new CompleteChunkedUploadResult(
            StorageKey: Domain.ValueObjects.StorageKey.From(storageResult.StorageKey),
            MimeType: storageResult.DetectedMimeType,
            Size: storageResult.Size,
            Checksum: storageResult.Sha256Checksum);
    }
}
