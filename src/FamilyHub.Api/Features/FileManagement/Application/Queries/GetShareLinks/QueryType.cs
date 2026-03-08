using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetShareLinks;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    public async Task<List<ShareLinkDto>> GetShareLinks(
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetShareLinksQuery();
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
