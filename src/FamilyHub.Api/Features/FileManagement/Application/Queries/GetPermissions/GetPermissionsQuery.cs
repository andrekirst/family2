using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Api.Features.FileManagement.Models;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Queries.GetPermissions;

public sealed record GetPermissionsQuery(
    PermissionResourceType ResourceType,
    Guid ResourceId,
    FamilyId FamilyId
) : IQuery<List<FilePermissionDto>>;
