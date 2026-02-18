using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Models;

namespace FamilyHub.Api.Features.FileManagement.Application.Mappers;

public static class FileManagementMapper
{
    public static StoredFileDto ToDto(StoredFile file) => new()
    {
        Id = file.Id.Value,
        Name = file.Name.Value,
        MimeType = file.MimeType.Value,
        Size = file.Size.Value,
        StorageKey = file.StorageKey.Value,
        Checksum = file.Checksum.Value,
        FolderId = file.FolderId.Value,
        FamilyId = file.FamilyId.Value,
        UploadedBy = file.UploadedBy.Value,
        CreatedAt = file.CreatedAt,
        UpdatedAt = file.UpdatedAt
    };

    public static FolderDto ToDto(Folder folder) => new()
    {
        Id = folder.Id.Value,
        Name = folder.Name.Value,
        ParentFolderId = folder.ParentFolderId?.Value,
        MaterializedPath = folder.MaterializedPath,
        FamilyId = folder.FamilyId.Value,
        CreatedBy = folder.CreatedBy.Value,
        CreatedAt = folder.CreatedAt
    };
}
