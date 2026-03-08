using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.MoveFile;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<StoredFileDto> MoveFile(
        MoveFileRequest input,
        [Service] ICommandBus commandBus,
        [Service] IStoredFileRepository storedFileRepository,
        CancellationToken cancellationToken)
    {
        var command = new MoveFileCommand(
            FileId.From(input.FileId),
            FolderId.From(input.TargetFolderId));

        var result = await commandBus.SendAsync(command, cancellationToken);

        var file = await storedFileRepository.GetByIdAsync(result.FileId, cancellationToken)
            ?? throw new InvalidOperationException("File move failed");

        return FileManagementMapper.ToDto(file);
    }
}
