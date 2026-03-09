using FamilyHub.Api.Common.Infrastructure.GraphQL;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFile;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<object> MoveFile(
        MoveFileRequest input,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new MoveFileCommand(
            FileId.From(input.FileId),
            FolderId.From(input.TargetFolderId));

        var result = await commandBus.SendAsync(command, cancellationToken);
        return result.Match<object>(
            success => FileManagementMapper.ToDto(success.MovedFile),
            error => MutationError.FromDomainError(error));
    }
}
