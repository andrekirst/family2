using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.UploadAvatar;

/// <summary>
/// Result of avatar upload command.
/// </summary>
public sealed record UploadAvatarResult(AvatarId AvatarId);
