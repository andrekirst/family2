using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetOrganizationRules;

[ExtendObjectType(typeof(FileManagementQuery))]
public class QueryType
{
    [Authorize]
    public async Task<List<OrganizationRuleDto>> GetOrganizationRules(
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetOrganizationRulesQuery();
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
