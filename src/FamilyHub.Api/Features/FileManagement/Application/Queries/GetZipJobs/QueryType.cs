using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetZipJobs;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    [HotChocolate.Types.UsePaging]
    public async Task<List<ZipJobDto>> GetZipJobs(
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetZipJobsQuery();
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
