using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteFolder;

public sealed class DeleteFolderCommandHandler(
    IFolderRepository folderRepository,
    IStoredFileRepository storedFileRepository,
    IFileManagementStorageService storageService)
    : ICommandHandler<DeleteFolderCommand, DeleteFolderResult>
{
    public async ValueTask<DeleteFolderResult> Handle(
        DeleteFolderCommand command,
        CancellationToken cancellationToken)
    {
        var folder = await folderRepository.GetByIdAsync(command.FolderId, cancellationToken)
            ?? throw new DomainException("Folder not found", DomainErrorCodes.FolderNotFound);

        if (folder.FamilyId != command.FamilyId)
        {
            throw new DomainException("Folder belongs to a different family", DomainErrorCodes.Forbidden);
        }

        // Prevent deleting root folder
        if (folder.ParentFolderId is null)
        {
            throw new DomainException("Cannot delete the root folder", DomainErrorCodes.Forbidden);
        }

        // Get all descendant folders
        var folderPath = folder.MaterializedPath + folder.Id.Value + "/";
        var descendants = await folderRepository.GetDescendantsAsync(folderPath, command.FamilyId, cancellationToken);

        // Collect all folder IDs (target + descendants)
        var allFolderIds = descendants.Select(d => d.Id).Append(folder.Id).ToList();

        // Get all files in all affected folders
        var files = await storedFileRepository.GetByFolderIdsAsync(allFolderIds, cancellationToken);

        // Delete binary data for all files
        foreach (var file in files)
        {
            await storageService.DeleteFileAsync(
                command.FamilyId, file.StorageKey.Value, file.Size.Value, cancellationToken);
            file.MarkDeleted(command.DeletedBy);
        }

        // Remove all files
        await storedFileRepository.RemoveRangeAsync(files, cancellationToken);

        // Raise domain event on the folder being deleted
        folder.MarkDeleted(command.DeletedBy);

        // Remove all descendant folders, then the folder itself
        await folderRepository.RemoveRangeAsync(descendants, cancellationToken);
        await folderRepository.RemoveAsync(folder, cancellationToken);

        return new DeleteFolderResult(true);
    }
}
