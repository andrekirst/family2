using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RemovePermission;

public sealed class RemovePermissionCommandHandler(
    IFilePermissionRepository permissionRepository)
    : ICommandHandler<RemovePermissionCommand, Result<RemovePermissionResult>>
{
    public async ValueTask<Result<RemovePermissionResult>> Handle(
        RemovePermissionCommand command,
        CancellationToken cancellationToken)
    {
        var permission = await permissionRepository.GetByMemberAndResourceAsync(
            command.MemberId, command.ResourceType, command.ResourceId, cancellationToken);
        if (permission is null)
        {
            return DomainError.NotFound(DomainErrorCodes.NotFound, "Permission not found");
        }

        if (permission.FamilyId != command.FamilyId)
        {
            return DomainError.Forbidden(DomainErrorCodes.Forbidden, "Permission belongs to a different family");
        }

        await permissionRepository.RemoveAsync(permission, cancellationToken);

        return new RemovePermissionResult(true);
    }
}
