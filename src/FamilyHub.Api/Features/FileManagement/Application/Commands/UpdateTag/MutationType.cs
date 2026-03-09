using FamilyHub.Api.Common.Infrastructure.GraphQL;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UpdateTag;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<object> UpdateTag(
        UpdateTagRequest input,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTagCommand(
            TagId.From(input.TagId),
            input.Name is not null ? TagName.From(input.Name.Trim()) : null,
            input.Color is not null ? TagColor.From(input.Color.Trim()) : null);

        var result = await commandBus.SendAsync(command, cancellationToken);
        return result.Match<object>(
            success => FileManagementMapper.ToDto(success.UpdatedTag),
            error => MutationError.FromDomainError(error));
    }
}
