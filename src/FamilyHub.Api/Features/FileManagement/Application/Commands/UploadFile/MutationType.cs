using System.Security.Claims;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
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
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        [Service] IStoredFileRepository storedFileRepository,
        CancellationToken cancellationToken)
    {
        var externalUserIdString = claimsPrincipal.FindFirst(ClaimNames.Sub)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var user = await userRepository.GetByExternalIdAsync(
            ExternalUserId.From(externalUserIdString), cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found");

        var familyId = user.FamilyId
            ?? throw new UnauthorizedAccessException("User is not a member of any family");

        var command = new UploadFileCommand(
            FileName.From(input.Name.Trim()),
            MimeType.From(input.MimeType.Trim()),
            FileSize.From(input.Size),
            StorageKey.From(input.StorageKey.Trim()),
            Checksum.From(input.Checksum.Trim()),
            FolderId.From(input.FolderId),
            familyId,
            user.Id);

        var result = await commandBus.SendAsync(command, cancellationToken);

        var file = await storedFileRepository.GetByIdAsync(result.FileId, cancellationToken)
            ?? throw new InvalidOperationException("File upload failed");

        return FileManagementMapper.ToDto(file);
    }
}
