using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
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
    public async Task<TagDto> UpdateTag(
        UpdateTagRequest input,
        [Service] ICommandBus commandBus,
        [Service] ITagRepository tagRepository,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTagCommand(
            TagId.From(input.TagId),
            input.Name is not null ? TagName.From(input.Name.Trim()) : null,
            input.Color is not null ? TagColor.From(input.Color.Trim()) : null);

        var result = await commandBus.SendAsync(command, cancellationToken);

        var tag = await tagRepository.GetByIdAsync(result.TagId, cancellationToken)
            ?? throw new InvalidOperationException("Tag update failed");

        return FileManagementMapper.ToDto(tag);
    }
}
