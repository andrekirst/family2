using System.Security.Claims;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.SearchFiles;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    public async Task<List<FileSearchResultDto>> SearchFiles(
        string query,
        List<string>? mimeTypes,
        DateTime? dateFrom,
        DateTime? dateTo,
        List<Guid>? tagIds,
        Guid? folderId,
        double? gpsLatitude,
        double? gpsLongitude,
        double? gpsRadiusKm,
        string sortBy = "relevance",
        int skip = 0,
        int take = 20,
        ClaimsPrincipal? claimsPrincipal = null,
        [Service] IQueryBus? queryBus = null,
        [Service] IUserRepository? userRepository = null,
        CancellationToken cancellationToken = default)
    {
        var externalUserIdString = claimsPrincipal!.FindFirst(ClaimNames.Sub)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var user = await userRepository!.GetByExternalIdAsync(
            ExternalUserId.From(externalUserIdString), cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found");

        var familyId = user.FamilyId
            ?? throw new UnauthorizedAccessException("User is not a member of any family");

        var filters = new SearchFiltersDto
        {
            MimeTypes = mimeTypes,
            DateFrom = dateFrom,
            DateTo = dateTo,
            TagIds = tagIds,
            FolderId = folderId,
            GpsLatitude = gpsLatitude,
            GpsLongitude = gpsLongitude,
            GpsRadiusKm = gpsRadiusKm
        };

        var searchQuery = new SearchFilesQuery(
            query, familyId, user.Id, filters, sortBy, skip, take);

        return await queryBus!.QueryAsync(searchQuery, cancellationToken);
    }
}
