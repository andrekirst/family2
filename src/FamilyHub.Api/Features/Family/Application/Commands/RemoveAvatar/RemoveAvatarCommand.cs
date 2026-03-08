using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Commands.RemoveAvatar;

/// <summary>
/// Command to remove the current user's global avatar.
/// </summary>
public sealed record RemoveAvatarCommand : ICommand<RemoveAvatarResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
