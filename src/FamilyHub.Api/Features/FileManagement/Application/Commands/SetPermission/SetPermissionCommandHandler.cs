using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.SetPermission;

public sealed class SetPermissionCommandHandler(
    IFilePermissionRepository permissionRepository,
    IStoredFileRepository storedFileRepository,
    IFolderRepository folderRepository,
    TimeProvider timeProvider)
    : ICommandHandler<SetPermissionCommand, Result<SetPermissionResult>>
{
    public async ValueTask<Result<SetPermissionResult>> Handle(
        SetPermissionCommand command,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        if (command.ResourceType == PermissionResourceType.File)
        {
            var file = await storedFileRepository.GetByIdAsync(
                FileId.From(command.ResourceId), cancellationToken);
            if (file is null)
            {
                return DomainError.NotFound(DomainErrorCodes.FileNotFound, "File not found");
            }

            if (file.FamilyId != command.FamilyId)
            {
                return DomainError.Forbidden(DomainErrorCodes.Forbidden, "File belongs to a different family");
            }
        }
        else
        {
            var folder = await folderRepository.GetByIdAsync(
                FolderId.From(command.ResourceId), cancellationToken);
            if (folder is null)
            {
                return DomainError.NotFound(DomainErrorCodes.FolderNotFound, "Folder not found");
            }

            if (folder.FamilyId != command.FamilyId)
            {
                return DomainError.Forbidden(DomainErrorCodes.Forbidden, "Folder belongs to a different family");
            }
        }

        var existing = await permissionRepository.GetByMemberAndResourceAsync(
            command.MemberId, command.ResourceType, command.ResourceId, cancellationToken);

        if (existing is not null)
        {
            existing.UpdateLevel(command.PermissionLevel, command.UserId, utcNow);
            return new SetPermissionResult(true, existing.Id.Value);
        }

        var permission = FilePermission.Create(
            command.ResourceType,
            command.ResourceId,
            command.MemberId,
            command.PermissionLevel,
            command.FamilyId,
            command.UserId,
            utcNow);

        await permissionRepository.AddAsync(permission, cancellationToken);

        return new SetPermissionResult(true, permission.Id.Value);
    }
}
