using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFolder;

public sealed class MoveFolderCommandHandler(
    IFolderRepository folderRepository,
    TimeProvider timeProvider)
    : ICommandHandler<MoveFolderCommand, Result<MoveFolderResult>>
{
    public async ValueTask<Result<MoveFolderResult>> Handle(
        MoveFolderCommand command,
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

        var targetParent = await folderRepository.GetByIdAsync(command.TargetParentFolderId, cancellationToken);
        if (targetParent is null)
        {
            return DomainError.NotFound(DomainErrorCodes.FolderNotFound, "Target parent folder not found");
        }

        if (targetParent.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "Target folder belongs to a different family");
        }

        if (command.TargetParentFolderId == command.FolderId)
        {
            return DomainError.BusinessRule(DomainErrorCodes.Forbidden, "Cannot move a folder into itself");
        }

        var oldPath = folder.MaterializedPath + folder.Id.Value + "/";
        if (targetParent.MaterializedPath.StartsWith(oldPath) || targetParent.Id == folder.Id)
        {
            return DomainError.BusinessRule(DomainErrorCodes.Forbidden, "Cannot move a folder into one of its descendants");
        }

        var newParentPath = targetParent.MaterializedPath == "/"
            ? $"/{targetParent.Id.Value}/"
            : $"{targetParent.MaterializedPath}{targetParent.Id.Value}/";

        var currentFolderPath = folder.MaterializedPath + folder.Id.Value + "/";
        var descendants = await folderRepository.GetDescendantsAsync(currentFolderPath, command.FamilyId, cancellationToken);

        folder.MoveTo(command.TargetParentFolderId, newParentPath, command.UserId, utcNow);

        var newFolderPath = newParentPath + folder.Id.Value + "/";
        foreach (var descendant in descendants)
        {
            var updatedPath = descendant.MaterializedPath.Replace(currentFolderPath, newFolderPath);
            descendant.UpdateMaterializedPath(updatedPath, utcNow);
        }

        return new MoveFolderResult(folder.Id, folder);
    }
}
