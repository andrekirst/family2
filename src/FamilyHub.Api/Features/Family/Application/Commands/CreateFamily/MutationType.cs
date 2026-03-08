using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Family.Application.Mappers;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Models;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Family.Application.Commands.CreateFamily;

[ExtendObjectType(typeof(FamilyMutation))]
public class MutationType
{
    /// <summary>
    /// Create a new family with the current user as owner.
    /// </summary>
    [Authorize]
    public async Task<FamilyDto> Create(
        CreateFamilyRequest input,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var familyName = FamilyName.From(input.Name.Trim());
        var command = new CreateFamilyCommand(familyName);
        var result = await commandBus.SendAsync(command, cancellationToken);

        return FamilyMapper.ToDto(result.CreatedFamily);
    }
}
