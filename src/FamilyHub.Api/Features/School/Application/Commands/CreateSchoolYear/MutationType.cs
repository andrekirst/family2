using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.BaseData.Domain.ValueObjects;
using FamilyHub.Api.Features.School.Application.Mappers;
using FamilyHub.Api.Features.School.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.School.Application.Commands.CreateSchoolYear;

[ExtendObjectType(typeof(SchoolMutation))]
public class MutationType
{
    [Authorize]
    public async Task<SchoolYearDto> CreateSchoolYear(
        CreateSchoolYearRequest input,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new CreateSchoolYearCommand(
            FederalStateId.From(input.FederalStateId),
            input.StartYear,
            input.EndYear,
            input.StartDate,
            input.EndDate);

        var result = await commandBus.SendAsync(command, cancellationToken);

        return result.Match(
            success => SchoolYearMapper.ToDto(success.CreatedSchoolYear, DateOnly.FromDateTime(DateTime.UtcNow)),
            error => throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage(error.Message)
                    .SetCode(error.ErrorCode)
                    .Build()));
    }
}
