using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.FileManagement.Application.Mappers;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.UploadFile;

[ExtendObjectType(typeof(FileManagementMutation))]
public class MutationType
{
    [Authorize]
    public async Task<StoredFileDto> UploadFile(
        UploadFileRequest input,
        [Service] ICommandBus commandBus,
        [Service] IStoredFileRepository storedFileRepository,
        CancellationToken cancellationToken)
    {
        var command = new UploadFileCommand(
            FileName.From(input.Name.Trim()),
            MimeType.From(input.MimeType.Trim()),
            FileSize.From(input.Size),
            StorageKey.From(input.StorageKey.Trim()),
            Checksum.From(input.Checksum.Trim()),
            FolderId.From(input.FolderId));

        var result = await commandBus.SendAsync(command, cancellationToken);

        var file = await storedFileRepository.GetByIdAsync(result.FileId, cancellationToken)
            ?? throw new InvalidOperationException("File upload failed");

        return FileManagementMapper.ToDto(file);
    }
}
