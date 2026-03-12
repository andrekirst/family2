using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.School.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.School.Application.Queries.GetSchoolYears;

[ExtendObjectType(typeof(SchoolQuery))]
public class QueryType
{
    [Authorize]
    public async Task<List<SchoolYearDto>> GetSchoolYears(
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetSchoolYearsQuery();
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
