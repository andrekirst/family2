using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RenameFolder;

public sealed class RenameFolderCommandHandler(
    IFolderRepository folderRepository,
    TimeProvider timeProvider)
    : ICommandHandler<RenameFolderCommand, Result<RenameFolderResult>>
{
    public async ValueTask<Result<RenameFolderResult>> Handle(
        RenameFolderCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var folder = await folderRepository.GetByIdAsync(command.FolderId, cancellationToken);
        if (folder is null)
        {
            return DomainError.NotFound(DomainErrorCodes.FolderNotFound, "Folder not found");
        }

        if (folder.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "Folder belongs to a different family");
        }

        folder.Rename(command.NewName, utcNow);

        return new RenameFolderResult(folder.Id, folder);
    }
}
