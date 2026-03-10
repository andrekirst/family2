using FamilyHub.Api.Common.Infrastructure.BlobStaging;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.StoreUploadedFile;

/// <summary>
/// Stores a file via IFileManagementStorageService (MIME detection, checksum, quota enforcement)
/// and returns the storage result metadata. FamilyId is populated by UserResolutionBehavior.
/// A blob staging entry is written in the same transaction for the outbox pattern.
/// </summary>
public sealed class StoreUploadedFileCommandHandler(
    IFileManagementStorageService storageService,
    IBlobStagingRepository blobStagingRepository)
    : ICommandHandler<StoreUploadedFileCommand, Result<StoreUploadedFileResult>>
{
    public async ValueTask<Result<StoreUploadedFileResult>> Handle(
        StoreUploadedFileCommand command,
        CancellationToken cancellationToken)
    {
        var storageResult = await storageService.StoreFileAsync(
            command.FamilyId, command.FileStream, command.FileName, cancellationToken);

        // Write blob staging entry in same transaction for outbox pattern
        await blobStagingRepository.AddAsync(new BlobStagingEntry
        {
            Module = "FileManagement",
            StorageKey = storageResult.StorageKey,
            MaxRetries = 5
        }, cancellationToken);

        return new StoreUploadedFileResult(
            StorageKey: Domain.ValueObjects.StorageKey.From(storageResult.StorageKey),
            MimeType: storageResult.DetectedMimeType,
            FileSize: storageResult.Size,
            Checksum: storageResult.Sha256Checksum);
    }
}
