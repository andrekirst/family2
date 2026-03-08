using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.SetPermission;

public sealed record SetPermissionCommand(
    PermissionResourceType ResourceType,
    Guid ResourceId,
    UserId MemberId,
    FilePermissionLevel PermissionLevel
) : ICommand<SetPermissionResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
