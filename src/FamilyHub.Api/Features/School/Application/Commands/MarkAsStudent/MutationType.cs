using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Mappers;
using FamilyHub.Api.Features.School.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.School.Application.Commands.MarkAsStudent;

[ExtendObjectType(typeof(SchoolMutation))]
public class MutationType
{
    [Authorize]
    public async Task<object> MarkAsStudent(
        MarkAsStudentRequest input,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var familyMemberId = FamilyMemberId.From(input.FamilyMemberId);
        var command = new MarkAsStudentCommand(familyMemberId);
        var result = await commandBus.SendAsync(command, cancellationToken);

        return result.Match<object>(
            success => StudentMapper.ToDto(success.CreatedStudent),
            error => MutationError.FromDomainError(error));
    }
}
