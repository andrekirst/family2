using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.SetFamilyAvatar;

/// <summary>
/// Command to set a per-family avatar override on a FamilyMember.
/// </summary>
public sealed record SetFamilyAvatarCommand(
    UserId UserId,
    FamilyId FamilyId,
    AvatarId AvatarId
) : ICommand<SetFamilyAvatarResult>;
