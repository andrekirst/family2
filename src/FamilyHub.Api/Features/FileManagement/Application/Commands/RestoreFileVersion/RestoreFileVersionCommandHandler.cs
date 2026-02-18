using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RestoreFileVersion;

public sealed class RestoreFileVersionCommandHandler(
    IFileVersionRepository versionRepository,
    IStoredFileRepository fileRepository)
    : ICommandHandler<RestoreFileVersionCommand, RestoreFileVersionResult>
{
    public async ValueTask<RestoreFileVersionResult> Handle(
        RestoreFileVersionCommand command,
        CancellationToken cancellationToken)
    {
        var file = await fileRepository.GetByIdAsync(command.FileId, cancellationToken)
            ?? throw new DomainException("File not found", DomainErrorCodes.FileNotFound);

        var sourceVersion = await versionRepository.GetByIdAsync(command.VersionId, cancellationToken)
            ?? throw new DomainException("Version not found", DomainErrorCodes.FileVersionNotFound);

        if (sourceVersion.FileId != command.FileId)
            throw new DomainException("Version does not belong to this file", DomainErrorCodes.FileVersionNotFound);

        // Mark current version as not current
        var currentVersion = await versionRepository.GetCurrentVersionAsync(command.FileId, cancellationToken);
        currentVersion?.MarkAsNotCurrent();

        // Create a new version from the restored version's data
        var maxVersion = await versionRepository.GetMaxVersionNumberAsync(command.FileId, cancellationToken);

        var newVersion = FileVersion.CreateFromRestore(
            command.FileId,
            maxVersion + 1,
            command.VersionId,
            sourceVersion.StorageKey,
            sourceVersion.FileSize,
            sourceVersion.Checksum,
            command.RestoredBy);

        await versionRepository.AddAsync(newVersion, cancellationToken);

        return new RestoreFileVersionResult(true, newVersion.Id.Value, newVersion.VersionNumber);
    }
}
