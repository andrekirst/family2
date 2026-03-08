using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetProcessingLog;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    public async Task<List<ProcessingLogEntryDto>> GetProcessingLog(
        int skip,
        int take,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetProcessingLogQuery(skip, take);
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
