using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UploadFile;

public sealed class UploadFileCommandHandler(
    IStoredFileRepository storedFileRepository,
    IFolderRepository folderRepository)
    : ICommandHandler<UploadFileCommand, UploadFileResult>
{
    public async ValueTask<UploadFileResult> Handle(
        UploadFileCommand command,
        CancellationToken cancellationToken)
    {
        // Validate folder exists and belongs to the same family
        var folder = await folderRepository.GetByIdAsync(command.FolderId, cancellationToken)
            ?? throw new DomainException("Folder not found", DomainErrorCodes.NotFound);

        if (folder.FamilyId != command.FamilyId)
        {
            throw new DomainException("Folder belongs to a different family", DomainErrorCodes.Forbidden);
        }

        var file = StoredFile.Create(
            command.Name,
            command.MimeType,
            command.Size,
            command.StorageKey,
            command.Checksum,
            command.FolderId,
            command.FamilyId,
            command.UploadedBy);

        await storedFileRepository.AddAsync(file, cancellationToken);

        return new UploadFileResult(file.Id);
    }
}
