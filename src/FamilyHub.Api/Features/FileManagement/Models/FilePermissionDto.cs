using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Models;

public sealed record FilePermissionDto(
    Guid Id,
    string ResourceType,
    Guid ResourceId,
    Guid MemberId,
    FilePermissionLevel PermissionLevel,
    Guid GrantedBy,
    DateTime GrantedAt);
