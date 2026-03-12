using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.School.Application.Commands.RemoveClassAssignment;

[ExtendObjectType(typeof(SchoolMutation))]
public class MutationType
{
    [Authorize]
    public async Task<bool> RemoveClassAssignment(
        RemoveClassAssignmentRequest input,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new RemoveClassAssignmentCommand(ClassAssignmentId.From(input.ClassAssignmentId));
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
