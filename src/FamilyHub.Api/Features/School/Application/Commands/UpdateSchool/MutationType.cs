using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Mappers;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.School.Application.Commands.UpdateSchool;

[ExtendObjectType(typeof(SchoolMutation))]
public class MutationType
{
    [Authorize]
    public async Task<SchoolDto> UpdateSchool(
        UpdateSchoolRequest input,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSchoolCommand(
            SchoolId.From(input.SchoolId),
            SchoolName.From(input.Name),
            FederalStateId.From(input.FederalStateId),
            input.City,
            input.PostalCode);

        var result = await commandBus.SendAsync(command, cancellationToken);

        return result.Match(
            success => SchoolMapper.ToDto(success.UpdatedSchool),
            error => throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(error.Message)
                    .SetCode(error.ErrorCode)
                    .Build()));
    }
}
