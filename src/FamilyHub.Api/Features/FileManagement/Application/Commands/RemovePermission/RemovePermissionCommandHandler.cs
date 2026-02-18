using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RemovePermission;

public sealed class RemovePermissionCommandHandler(
    IFilePermissionRepository permissionRepository)
    : ICommandHandler<RemovePermissionCommand, RemovePermissionResult>
{
    public async ValueTask<RemovePermissionResult> Handle(
        RemovePermissionCommand command,
        CancellationToken cancellationToken)
    {
        var permission = await permissionRepository.GetByMemberAndResourceAsync(
            command.MemberId, command.ResourceType, command.ResourceId, cancellationToken)
            ?? throw new DomainException("Permission not found", DomainErrorCodes.NotFound);

        if (permission.FamilyId != command.FamilyId)
            throw new DomainException("Permission belongs to a different family", DomainErrorCodes.Forbidden);

        await permissionRepository.RemoveAsync(permission, cancellationToken);

        return new RemovePermissionResult(true);
    }
}
