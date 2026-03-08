using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.SetFamilyAvatar;

/// <summary>
/// Command to set a per-family avatar override on a FamilyMember.
/// </summary>
public sealed record SetFamilyAvatarCommand(
    AvatarId AvatarId
) : ICommand<SetFamilyAvatarResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
