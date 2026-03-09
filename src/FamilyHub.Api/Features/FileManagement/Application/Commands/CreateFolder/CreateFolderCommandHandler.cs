using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateFolder;

public sealed class CreateFolderCommandHandler(
    IFolderRepository folderRepository,
    TimeProvider timeProvider)
    : ICommandHandler<CreateFolderCommand, Result<CreateFolderResult>>
{
    public async ValueTask<Result<CreateFolderResult>> Handle(
        CreateFolderCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        string materializedPath;
        FolderId? effectiveParentId = command.ParentFolderId;

        if (command.ParentFolderId is not null)
        {
            var parentFolder = await folderRepository.GetByIdAsync(command.ParentFolderId.Value, cancellationToken);
            if (parentFolder is null)
            {
                return DomainError.NotFound(DomainErrorCodes.NotFound, "Parent folder not found");
            }

            if (parentFolder.FamilyId != command.FamilyId)
            {
                return DomainError.Forbidden(DomainErrorCodes.Forbidden, "Parent folder belongs to a different family");
            }

            materializedPath = parentFolder.MaterializedPath == "/"
                ? $"/{parentFolder.Id.Value}/"
                : $"{parentFolder.MaterializedPath}{parentFolder.Id.Value}/";
        }
        else
        {
            var rootFolder = await folderRepository.GetRootFolderAsync(command.FamilyId, cancellationToken);

            if (rootFolder is null)
            {
                rootFolder = Folder.CreateRoot(command.FamilyId, command.UserId, utcNow);
                await folderRepository.AddAsync(rootFolder, cancellationToken);
            }

            effectiveParentId = rootFolder.Id;
            materializedPath = $"/{rootFolder.Id.Value}/";
        }

        var folder = Folder.Create(
            command.Name,
            effectiveParentId,
            materializedPath,
            command.FamilyId,
            command.UserId,
            utcNow);

        await folderRepository.AddAsync(folder, cancellationToken);

        return new CreateFolderResult(folder.Id, folder);
    }
}
