using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UploadFile;

public sealed class UploadFileCommandHandler(
    IStoredFileRepository storedFileRepository,
    IFolderRepository folderRepository,
    TimeProvider timeProvider)
    : ICommandHandler<UploadFileCommand, Result<UploadFileResult>>
{
    public async ValueTask<Result<UploadFileResult>> Handle(
        UploadFileCommand command,
        CancellationToken cancellationToken)
    {
        var folder = await folderRepository.GetByIdAsync(command.FolderId, cancellationToken);
        if (folder is null)
        {
            return DomainError.NotFound(DomainErrorCodes.NotFound, "Folder not found");
        }

        if (folder.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "Folder belongs to a different family");
        }

        var utcNow = timeProvider.GetUtcNow();
        var file = StoredFile.Create(
            command.Name,
            command.MimeType,
            command.Size,
            command.StorageKey,
            command.Checksum,
            command.FolderId,
            command.FamilyId,
            command.UserId,
            utcNow);

        await storedFileRepository.AddAsync(file, cancellationToken);

        return new UploadFileResult(file.Id, file);
    }
}
