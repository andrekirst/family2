using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.School.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.School.Application.Queries.GetStudents;

[ExtendObjectType(typeof(SchoolQuery))]
public class QueryType
{
    [Authorize]
    public async Task<List<StudentDto>> GetStudents(
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetStudentsQuery();
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
