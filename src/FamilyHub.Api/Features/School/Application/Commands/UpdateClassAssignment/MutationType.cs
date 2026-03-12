using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.School.Application.Mappers;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.School.Application.Commands.UpdateClassAssignment;

[ExtendObjectType(typeof(SchoolMutation))]
public class MutationType
{
    [Authorize]
    public async Task<ClassAssignmentDto> UpdateClassAssignment(
        UpdateClassAssignmentRequest input,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new UpdateClassAssignmentCommand(
            ClassAssignmentId.From(input.ClassAssignmentId),
            SchoolId.From(input.SchoolId),
            SchoolYearId.From(input.SchoolYearId),
            ClassName.From(input.ClassName));

        var result = await commandBus.SendAsync(command, cancellationToken);

        return result.Match(
            success => ClassAssignmentMapper.ToDto(success.UpdatedAssignment, string.Empty, false),
            error => throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(error.Message)
                    .SetCode(error.ErrorCode)
                    .Build()));
    }
}
