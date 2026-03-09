using FamilyHub.Api.Common.Infrastructure.GraphQL;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetPermissions;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    [HotChocolate.Types.UsePaging]
    public async Task<object> GetPermissions(
        string resourceType,
        Guid resourceId,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        PermissionResourceType parsedResourceType;
        switch (resourceType.ToLowerInvariant())
        {
            case "file":
                parsedResourceType = PermissionResourceType.File;
                break;
            case "folder":
                parsedResourceType = PermissionResourceType.Folder;
                break;
            default:
                var error = DomainError.Validation(
                    "INVALID_RESOURCE_TYPE",
                    $"Invalid resource type: {resourceType}");
                return MutationError.FromDomainError(error);
        }

        var query = new GetPermissionsQuery(parsedResourceType, resourceId);
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
