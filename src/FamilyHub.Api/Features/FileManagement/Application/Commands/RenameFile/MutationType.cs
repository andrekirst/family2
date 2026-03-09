using FamilyHub.Api.Common.Infrastructure.GraphQL;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RenameFile;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<object> RenameFile(
        RenameFileRequest input,
        [Service] ICommandBus commandBus,
        CancellationToken cancellationToken)
    {
        var command = new RenameFileCommand(
            FileId.From(input.FileId),
            FileName.From(input.NewName.Trim()));

        var result = await commandBus.SendAsync(command, cancellationToken);
        return result.Match<object>(
            success => FileManagementMapper.ToDto(success.RenamedFile),
            error => MutationError.FromDomainError(error));
    }
}
