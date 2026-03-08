using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetPermissions;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    public async Task<List<FilePermissionDto>> GetPermissions(
        string resourceType,
        Guid resourceId,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var parsedResourceType = resourceType.ToLowerInvariant() switch
        {
            "file" => PermissionResourceType.File,
            "folder" => PermissionResourceType.Folder,
            _ => throw new ArgumentException($"Invalid resource type: {resourceType}")
        };

        var query = new GetPermissionsQuery(parsedResourceType, resourceId);
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
