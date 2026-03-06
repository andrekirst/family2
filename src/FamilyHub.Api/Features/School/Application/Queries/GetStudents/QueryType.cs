using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.School.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.School.Application.Queries.GetStudents;

[ExtendObjectType(typeof(SchoolQuery))]
public class QueryType
{
    [Authorize]
    public async Task<List<StudentDto>> GetStudents(
        ClaimsPrincipal claimsPrincipal,
        [Service] IQueryBus queryBus,
        [Service] IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var externalUserIdString = claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value
                                   ?? throw new UnauthorizedAccessException("User not authenticated");

        var externalUserId = ExternalUserId.From(externalUserIdString);
        var user = await userRepository.GetByExternalIdAsync(externalUserId, cancellationToken)
                   ?? throw new UnauthorizedAccessException("User not found");

        if (user.FamilyId is null)
        {
            return [];
        }

        var query = new GetStudentsQuery(user.FamilyId.Value);
        return await queryBus.QueryAsync(query, cancellationToken);
    }
}
