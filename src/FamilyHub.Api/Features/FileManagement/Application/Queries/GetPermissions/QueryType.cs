using System.Security.Claims;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetPermissions;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    public async Task<List<FilePermissionDto>> GetPermissions(
        string resourceType,
        Guid resourceId,
        ClaimsPrincipal claimsPrincipal,
        [Service] IQueryBus queryBus,
        [Service] IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var externalUserIdString = claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var user = await userRepository.GetByExternalIdAsync(
            ExternalUserId.From(externalUserIdString), cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found");

        var familyId = user.FamilyId
            ?? throw new UnauthorizedAccessException("User is not a member of any family");

        var parsedResourceType = resourceType.ToLowerInvariant() switch
        {
            "file" => PermissionResourceType.File,
            "folder" => PermissionResourceType.Folder,
            _ => throw new ArgumentException($"Invalid resource type: {resourceType}")
        };

        var query = new GetPermissionsQuery(parsedResourceType, resourceId, familyId);
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
