using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Mappers;
using FamilyHub.Api.Features.School.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.School.Application.Commands.UpdateSchoolYear;

[ExtendObjectType(typeof(SchoolMutation))]
public class MutationType
{
    [Authorize]
    public async Task<SchoolYearDto> UpdateSchoolYear(
        UpdateSchoolYearRequest input,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSchoolYearCommand(
            SchoolYearId.From(input.SchoolYearId),
            FederalStateId.From(input.FederalStateId),
            input.StartYear,
            input.EndYear,
            input.StartDate,
            input.EndDate);

        var result = await commandBus.SendAsync(command, cancellationToken);

        return result.Match(
            success => SchoolYearMapper.ToDto(success.UpdatedSchoolYear, DateOnly.FromDateTime(DateTime.UtcNow)),
            error => throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(error.Message)
                    .SetCode(error.ErrorCode)
                    .Build()));
    }
}
