using FamilyHub.Api.Common.Infrastructure.GraphQL;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateTag;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<object> CreateTag(
        CreateTagRequest input,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new CreateTagCommand(
            TagName.From(input.Name.Trim()),
            TagColor.From(input.Color.Trim()));

        var result = await commandBus.SendAsync(command, cancellationToken);
        return result.Match<object>(
            success => FileManagementMapper.ToDto(success.CreatedTag),
            error => MutationError.FromDomainError(error));
    }
}
