using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateFileVersion;

public sealed class CreateFileVersionCommandHandler(
    IFileVersionRepository versionRepository,
    IStoredFileRepository fileRepository)
    : ICommandHandler<CreateFileVersionCommand, CreateFileVersionResult>
{
    public async ValueTask<CreateFileVersionResult> Handle(
        CreateFileVersionCommand command,
        CancellationToken cancellationToken)
    {
        var file = await fileRepository.GetByIdAsync(command.FileId, cancellationToken)
            ?? throw new DomainException("File not found", DomainErrorCodes.FileNotFound);

        // Mark current version as not current
        var currentVersion = await versionRepository.GetCurrentVersionAsync(command.FileId, cancellationToken);
        currentVersion?.MarkAsNotCurrent();

        // Determine next version number
        var maxVersion = await versionRepository.GetMaxVersionNumberAsync(command.FileId, cancellationToken);

        var version = FileVersion.Create(
            command.FileId,
            maxVersion + 1,
            command.StorageKey,
            command.FileSize,
            command.Checksum,
            command.UploadedBy);

        await versionRepository.AddAsync(version, cancellationToken);

        return new CreateFileVersionResult(true, version.Id.Value, version.VersionNumber);
    }
}
