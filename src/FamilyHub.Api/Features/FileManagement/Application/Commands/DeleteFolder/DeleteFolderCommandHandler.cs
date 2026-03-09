using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Infrastructure.Storage;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteFolder;

public sealed class DeleteFolderCommandHandler(
    IFolderRepository folderRepository,
    IStoredFileRepository storedFileRepository,
    IFileManagementStorageService storageService,
    TimeProvider timeProvider)
    : ICommandHandler<DeleteFolderCommand, Result<DeleteFolderResult>>
{
    public async ValueTask<Result<DeleteFolderResult>> Handle(
        DeleteFolderCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var folder = (await folderRepository.GetByIdAsync(command.FolderId, cancellationToken))!;

        if (folder.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "Folder belongs to a different family");
        }

        if (folder.ParentFolderId is null)
        {
            return DomainError.BusinessRule(DomainErrorCodes.Forbidden, "Cannot delete the root folder");
        }

        var folderPath = folder.MaterializedPath + folder.Id.Value + "/";
        var descendants = await folderRepository.GetDescendantsAsync(folderPath, command.FamilyId, cancellationToken);

        var allFolderIds = descendants.Select(d => d.Id).Append(folder.Id).ToList();

        var files = await storedFileRepository.GetByFolderIdsAsync(allFolderIds, cancellationToken);

        foreach (var file in files)
        {
            await storageService.DeleteFileAsync(
                command.FamilyId, file.StorageKey.Value, file.Size.Value, cancellationToken);
            file.MarkDeleted(command.UserId);
        }

        await storedFileRepository.RemoveRangeAsync(files, cancellationToken);

        folder.MarkDeleted(command.UserId, utcNow);

        await folderRepository.RemoveRangeAsync(descendants, cancellationToken);
        await folderRepository.RemoveAsync(folder, cancellationToken);

        return new DeleteFolderResult(true);
    }
}
