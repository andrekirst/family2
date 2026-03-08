using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Family.Application.Mappers;
using FamilyHub.Api.Features.Family.Domain.Repositories;
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
        [Service] IFamilyRepository familyRepository,
        CancellationToken cancellationToken)
    {
        var familyName = FamilyName.From(input.Name.Trim());
        var command = new CreateFamilyCommand(familyName);
        var result = await commandBus.SendAsync(command, cancellationToken);

        var createdFamily = await familyRepository.GetByIdWithMembersAsync(result.FamilyId, cancellationToken);
        return createdFamily is null
            ? throw new InvalidOperationException("Family creation failed")
            : FamilyMapper.ToDto(createdFamily);
    }
}
