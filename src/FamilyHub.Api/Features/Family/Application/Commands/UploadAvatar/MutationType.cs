using System.Security.Claims;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Family.Application.Commands.UploadAvatar;

[ExtendObjectType(typeof(FamilyMutation))]
public class MutationType
{
    /// <summary>
    /// Upload a new avatar image for the current user.
    /// Accepts Base64-encoded image data with optional crop coordinates.
    /// </summary>
    [Authorize]
    public async Task<UploadAvatarResultDto> UploadAvatar(
        UploadAvatarInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetCurrentUser(claimsPrincipal, userRepository, cancellationToken);

        if (user.FamilyId is null)
        {
            throw new InvalidOperationException("You must be part of a family to upload an avatar");
        }

        var imageData = Convert.FromBase64String(input.ImageBase64);

        var command = new UploadAvatarCommand(
            user.Id,
            imageData,
            input.FileName,
            input.MimeType,
            input.CropX,
            input.CropY,
            input.CropWidth,
            input.CropHeight,
            user.FamilyId.Value);

        var result = await commandBus.SendAsync(command, cancellationToken);

        return new UploadAvatarResultDto(result.AvatarId.Value);
    }
}

public record UploadAvatarInput(
    string ImageBase64,
    string FileName,
    string MimeType,
    float? CropX = null,
    float? CropY = null,
    float? CropWidth = null,
    float? CropHeight = null);

public record UploadAvatarResultDto(Guid AvatarId);
