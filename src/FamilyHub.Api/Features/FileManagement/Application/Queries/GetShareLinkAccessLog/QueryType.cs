using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetShareLinkAccessLog;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    [UsePaging]
    public async Task<List<ShareLinkAccessLogDto>> GetShareLinkAccessLog(
        Guid shareLinkId,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetShareLinkAccessLogQuery(
            ShareLinkId.From(shareLinkId));

        var result = await queryBus.QueryAsync(query, cancellationToken);
        return result.Match(
            success => success,
            error => throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(error.Message)
                    .SetCode(error.ErrorCode)
                    .Build()));
    }
}
