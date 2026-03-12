using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.School.Application.Queries.GetStudentClassAssignments;

[ExtendObjectType(typeof(SchoolQuery))]
public class QueryType
{
    [Authorize]
    public async Task<List<ClassAssignmentDto>> GetStudentClassAssignments(
        Guid studentId,
        [Service] IQueryBus queryBus,
        CancellationToken cancellationToken)
    {
        var query = new GetStudentClassAssignmentsQuery(StudentId.From(studentId));
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
