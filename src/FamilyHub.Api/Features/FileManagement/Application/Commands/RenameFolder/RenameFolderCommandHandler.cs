using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RenameFolder;

public sealed class RenameFolderCommandHandler(
    IFolderRepository folderRepository)
    : ICommandHandler<RenameFolderCommand, RenameFolderResult>
{
    public async ValueTask<RenameFolderResult> Handle(
        RenameFolderCommand command,
        CancellationToken cancellationToken)
    {
        var folder = await folderRepository.GetByIdAsync(command.FolderId, cancellationToken)
            ?? throw new DomainException("Folder not found", DomainErrorCodes.FolderNotFound);

        if (folder.FamilyId != command.FamilyId)
        {
            throw new DomainException("Folder belongs to a different family", DomainErrorCodes.Forbidden);
        }

        folder.Rename(command.NewName);

        return new RenameFolderResult(folder.Id);
    }
}
