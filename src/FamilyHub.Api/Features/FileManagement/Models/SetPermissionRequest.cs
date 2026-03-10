using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Models;

public sealed record SetPermissionRequest(
    PermissionResourceType ResourceType,
    Guid ResourceId,
    Guid MemberId,
    FilePermissionLevel PermissionLevel);
