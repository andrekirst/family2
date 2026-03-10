using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetExternalConnections;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    [HotChocolate.Types.UsePaging]
    public async Task<List<ExternalConnectionDto>> GetExternalConnections(
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetExternalConnectionsQuery();
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
