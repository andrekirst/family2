namespace FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

/// <summary>
/// Permission levels for file/folder access control.
/// Higher values include all lower permissions (Manage > Edit > View).
/// </summary>
public enum FilePermissionLevel
{
    View = 1,
    Edit = 2,
    Manage = 3
}
