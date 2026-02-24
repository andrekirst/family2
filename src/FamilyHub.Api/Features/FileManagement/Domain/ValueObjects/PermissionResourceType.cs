namespace FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;

/// <summary>
/// Discriminator for whether a permission targets a file or folder.
/// </summary>
public enum PermissionResourceType
{
    File = 1,
    Folder = 2
}
