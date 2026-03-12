using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.School.Application.Commands.DeleteSchool;

[ExtendObjectType(typeof(SchoolMutation))]
public class MutationType
{
    [Authorize]
    public async Task<bool> DeleteSchool(
        DeleteSchoolRequest input,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new DeleteSchoolCommand(SchoolId.From(input.SchoolId));
        var result = await commandBus.SendAsync(command, cancellationToken);

        return result.Match(
            success => success,
            error => throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(error.Message)
                    .SetCode(error.ErrorCode)
                    .Build()));
    }
}
