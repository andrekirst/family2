using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.RemovePermission;

public sealed record RemovePermissionCommand(
    PermissionResourceType ResourceType,
    Guid ResourceId,
    UserId MemberId,
    FamilyId FamilyId
) : ICommand<RemovePermissionResult>;
