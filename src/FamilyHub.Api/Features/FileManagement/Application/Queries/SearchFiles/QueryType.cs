using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.SearchFiles;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    [HotChocolate.Types.UsePaging]
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
        [Service] IQueryBus? queryBus = null,
        CancellationToken cancellationToken = default)
    {
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
            query, filters, sortBy, skip, take);

        return await queryBus!.QueryAsync(searchQuery, cancellationToken);
    }
}
