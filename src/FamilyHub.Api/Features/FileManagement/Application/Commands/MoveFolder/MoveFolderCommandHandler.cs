using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFolder;

public sealed class MoveFolderCommandHandler(
    IFolderRepository folderRepository)
    : ICommandHandler<MoveFolderCommand, MoveFolderResult>
{
    public async ValueTask<MoveFolderResult> Handle(
        MoveFolderCommand command,
        CancellationToken cancellationToken)
    {
        var folder = await folderRepository.GetByIdAsync(command.FolderId, cancellationToken)
            ?? throw new DomainException("Folder not found", DomainErrorCodes.FolderNotFound);

        if (folder.FamilyId != command.FamilyId)
        {
            throw new DomainException("Folder belongs to a different family", DomainErrorCodes.Forbidden);
        }

        var targetParent = await folderRepository.GetByIdAsync(command.TargetParentFolderId, cancellationToken)
            ?? throw new DomainException("Target parent folder not found", DomainErrorCodes.FolderNotFound);

        if (targetParent.FamilyId != command.FamilyId)
        {
            throw new DomainException("Target folder belongs to a different family", DomainErrorCodes.Forbidden);
        }

        // Prevent moving a folder into itself or its own descendants
        if (command.TargetParentFolderId == command.FolderId)
        {
            throw new DomainException("Cannot move a folder into itself", DomainErrorCodes.Forbidden);
        }

        var oldPath = folder.MaterializedPath + folder.Id.Value + "/";
        if (targetParent.MaterializedPath.StartsWith(oldPath) || targetParent.Id == folder.Id)
        {
            throw new DomainException("Cannot move a folder into one of its descendants", DomainErrorCodes.Forbidden);
        }

        // Calculate new materialized path for the moved folder
        var newParentPath = targetParent.MaterializedPath == "/"
            ? $"/{targetParent.Id.Value}/"
            : $"{targetParent.MaterializedPath}{targetParent.Id.Value}/";

        // Get all descendants before moving so we can update their paths
        var currentFolderPath = folder.MaterializedPath + folder.Id.Value + "/";
        var descendants = await folderRepository.GetDescendantsAsync(currentFolderPath, command.FamilyId, cancellationToken);

        // Move the folder itself
        folder.MoveTo(command.TargetParentFolderId, newParentPath, command.MovedBy);

        // Update all descendants' materialized paths
        var newFolderPath = newParentPath + folder.Id.Value + "/";
        foreach (var descendant in descendants)
        {
            var updatedPath = descendant.MaterializedPath.Replace(currentFolderPath, newFolderPath);
            descendant.UpdateMaterializedPath(updatedPath);
        }

        return new MoveFolderResult(folder.Id);
    }
}
