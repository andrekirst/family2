namespace FamilyHub.Api.Features.FileManagement.Models;

public sealed record SetPermissionRequest(
    string ResourceType,
    Guid ResourceId,
    Guid MemberId,
    int PermissionLevel);
