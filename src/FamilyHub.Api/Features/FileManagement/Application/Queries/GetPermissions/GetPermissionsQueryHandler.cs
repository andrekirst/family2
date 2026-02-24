using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetPermissions;

public sealed class GetPermissionsQueryHandler(
    IFilePermissionRepository permissionRepository)
    : IQueryHandler<GetPermissionsQuery, List<FilePermissionDto>>
{
    public async ValueTask<List<FilePermissionDto>> Handle(
        GetPermissionsQuery query,
        CancellationToken cancellationToken)
    {
        var permissions = await permissionRepository.GetByResourceAsync(
            query.ResourceType, query.ResourceId, cancellationToken);

        return permissions
            .Where(p => p.FamilyId == query.FamilyId)
            .Select(FileManagementMapper.ToDto)
            .ToList();
    }
}
