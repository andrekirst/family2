using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RestoreFileVersion;

public sealed class RestoreFileVersionCommandHandler(
    IFileVersionRepository versionRepository,
    IStoredFileRepository fileRepository,
    TimeProvider timeProvider)
    : ICommandHandler<RestoreFileVersionCommand, Result<RestoreFileVersionResult>>
{
    public async ValueTask<Result<RestoreFileVersionResult>> Handle(
        RestoreFileVersionCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var file = await fileRepository.GetByIdAsync(command.FileId, cancellationToken);
        if (file is null)
        {
            return DomainError.NotFound(DomainErrorCodes.FileNotFound, "File not found");
        }

        var sourceVersion = await versionRepository.GetByIdAsync(command.VersionId, cancellationToken);
        if (sourceVersion is null)
        {
            return DomainError.NotFound(DomainErrorCodes.FileVersionNotFound, "Version not found");
        }

        if (sourceVersion.FileId != command.FileId)
        {
            return DomainError.NotFound(DomainErrorCodes.FileVersionNotFound, "Version does not belong to this file");
        }

        var currentVersion = await versionRepository.GetCurrentVersionAsync(command.FileId, cancellationToken);
        currentVersion?.MarkAsNotCurrent();

        var maxVersion = await versionRepository.GetMaxVersionNumberAsync(command.FileId, cancellationToken);

        var newVersion = FileVersion.CreateFromRestore(
            command.FileId,
            maxVersion + 1,
            command.VersionId,
            sourceVersion.StorageKey,
            sourceVersion.FileSize,
            sourceVersion.Checksum,
            command.UserId,
            utcNow);

        await versionRepository.AddAsync(newVersion, cancellationToken);

        return new RestoreFileVersionResult(true, newVersion.Id.Value, newVersion.VersionNumber);
    }
}
