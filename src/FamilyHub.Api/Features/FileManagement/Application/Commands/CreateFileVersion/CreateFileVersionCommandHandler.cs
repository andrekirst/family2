using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateFileVersion;

public sealed class CreateFileVersionCommandHandler(
    IFileVersionRepository versionRepository,
    IStoredFileRepository fileRepository,
    TimeProvider timeProvider)
    : ICommandHandler<CreateFileVersionCommand, Result<CreateFileVersionResult>>
{
    public async ValueTask<Result<CreateFileVersionResult>> Handle(
        CreateFileVersionCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var file = await fileRepository.GetByIdAsync(command.FileId, cancellationToken);
        if (file is null)
        {
            return DomainError.NotFound(DomainErrorCodes.FileNotFound, "File not found");
        }

        var currentVersion = await versionRepository.GetCurrentVersionAsync(command.FileId, cancellationToken);
        currentVersion?.MarkAsNotCurrent();

        var maxVersion = await versionRepository.GetMaxVersionNumberAsync(command.FileId, cancellationToken);

        var version = FileVersion.Create(
            command.FileId,
            maxVersion + 1,
            command.StorageKey,
            command.FileSize,
            command.Checksum,
            command.UserId,
            utcNow);

        await versionRepository.AddAsync(version, cancellationToken);

        return new CreateFileVersionResult(true, version.Id.Value, version.VersionNumber);
    }
}
