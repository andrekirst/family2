using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Photos.Application.Commands;
using FamilyHub.Api.Features.Photos.Application.Mappers;
using FamilyHub.Api.Features.Photos.Domain.Repositories;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;
using FamilyHub.Api.Features.Photos.Models;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace FamilyHub.Api.Features.Photos.GraphQL;

[ExtendObjectType(typeof(FamilyPhotosMutation))]
public class PhotoMutations
{
    [Authorize]
    public async Task<PhotoDto> Upload(
        UploadPhotoRequest input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IPhotoRepository repository,
        [Service] IUserRepository userRepository,
        CancellationToken ct)
    {
        var externalUserIdString = claimsPrincipal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var externalUserId = ExternalUserId.From(externalUserIdString);
        var user = await userRepository.GetByExternalIdAsync(externalUserId, ct)
            ?? throw new UnauthorizedAccessException("User not found");

        if (user.FamilyId is null)
        {
            throw new InvalidOperationException("User must belong to a family to upload photos");
        }

        PhotoCaption? caption = !string.IsNullOrWhiteSpace(input.Caption)
            ? PhotoCaption.From(input.Caption.Trim())
            : null;

        var command = new UploadPhotoCommand(
            user.FamilyId.Value,
            user.Id,
            input.FileName.Trim(),
            input.ContentType.Trim(),
            input.FileSizeBytes,
            input.StoragePath.Trim(),
            caption);

        var result = await commandBus.SendAsync<UploadPhotoResult>(command, ct);

        var created = await repository.GetByIdAsync(result.PhotoId, ct)
            ?? throw new InvalidOperationException("Photo upload failed");

        return PhotoMapper.ToDto(created);
    }

    [Authorize]
    public async Task<PhotoDto> UpdateCaption(
        Guid id,
        UpdatePhotoCaptionRequest input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IPhotoRepository repository,
        [Service] IUserRepository userRepository,
        CancellationToken ct)
    {
        var externalUserIdString = claimsPrincipal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var externalUserId = ExternalUserId.From(externalUserIdString);
        _ = await userRepository.GetByExternalIdAsync(externalUserId, ct)
            ?? throw new UnauthorizedAccessException("User not found");

        var photoId = PhotoId.From(id);
        PhotoCaption? caption = !string.IsNullOrWhiteSpace(input.Caption)
            ? PhotoCaption.From(input.Caption.Trim())
            : null;

        var command = new UpdatePhotoCaptionCommand(photoId, caption);
        await commandBus.SendAsync<UpdatePhotoCaptionResult>(command, ct);

        var updated = await repository.GetByIdAsync(photoId, ct)
            ?? throw new InvalidOperationException("Photo update failed");

        return PhotoMapper.ToDto(updated);
    }

    [Authorize]
    public async Task<bool> Delete(
        Guid id,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        CancellationToken ct)
    {
        var externalUserIdString = claimsPrincipal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var externalUserId = ExternalUserId.From(externalUserIdString);
        var user = await userRepository.GetByExternalIdAsync(externalUserId, ct)
            ?? throw new UnauthorizedAccessException("User not found");

        var photoId = PhotoId.From(id);
        var command = new DeletePhotoCommand(photoId, user.Id);

        var result = await commandBus.SendAsync<DeletePhotoResult>(command, ct);
        return result.Success;
    }
}
