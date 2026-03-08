using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
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
    public async Task<TagDto> CreateTag(
        CreateTagRequest input,
        [Service] ICommandBus commandBus,
        [Service] ITagRepository tagRepository,
        CancellationToken cancellationToken)
    {
        var command = new CreateTagCommand(
            TagName.From(input.Name.Trim()),
            TagColor.From(input.Color.Trim()));

        var result = await commandBus.SendAsync(command, cancellationToken);

        var tag = await tagRepository.GetByIdAsync(result.TagId, cancellationToken)
            ?? throw new InvalidOperationException("Tag creation failed");

        return FileManagementMapper.ToDto(tag);
    }
}
