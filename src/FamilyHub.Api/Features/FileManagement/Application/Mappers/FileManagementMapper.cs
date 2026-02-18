using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Models;
using Tag = FamilyHub.Api.Features.FileManagement.Domain.Entities.Tag;

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

    public static TagDto ToDto(Tag tag, int fileCount = 0) => new()
    {
        Id = tag.Id.Value,
        Name = tag.Name.Value,
        Color = tag.Color.Value,
        FamilyId = tag.FamilyId.Value,
        FileCount = fileCount,
        CreatedAt = tag.CreatedAt
    };

    public static FileMetadataDto ToDto(FileMetadata metadata) => new()
    {
        FileId = metadata.FileId.Value,
        GpsLatitude = metadata.GpsLatitude?.Value,
        GpsLongitude = metadata.GpsLongitude?.Value,
        LocationName = metadata.LocationName,
        CameraModel = metadata.CameraModel,
        CaptureDate = metadata.CaptureDate,
        HasGpsData = metadata.HasGpsData
    };

    public static AlbumDto ToDto(Album album, int itemCount = 0) => new()
    {
        Id = album.Id.Value,
        Name = album.Name.Value,
        Description = album.Description,
        CoverFileId = album.CoverFileId?.Value,
        FamilyId = album.FamilyId.Value,
        CreatedBy = album.CreatedBy.Value,
        ItemCount = itemCount,
        CreatedAt = album.CreatedAt,
        UpdatedAt = album.UpdatedAt
    };

    public static FilePermissionDto ToDto(FilePermission permission) => new(
        Id: permission.Id.Value,
        ResourceType: permission.ResourceType.ToString(),
        ResourceId: permission.ResourceId,
        MemberId: permission.MemberId.Value,
        PermissionLevel: permission.PermissionLevel,
        GrantedBy: permission.GrantedBy.Value,
        GrantedAt: permission.GrantedAt);

    public static OrganizationRuleDto ToDto(OrganizationRule rule) => new(
        Id: rule.Id.Value,
        Name: rule.Name,
        ConditionsJson: rule.ConditionsJson,
        ConditionLogic: rule.ConditionLogic,
        ActionType: rule.ActionType,
        ActionsJson: rule.ActionsJson,
        Priority: rule.Priority,
        IsEnabled: rule.IsEnabled,
        CreatedBy: rule.CreatedBy.Value,
        CreatedAt: rule.CreatedAt,
        UpdatedAt: rule.UpdatedAt);

    public static ProcessingLogEntryDto ToDto(ProcessingLogEntry entry) => new(
        Id: entry.Id.Value,
        FileId: entry.FileId.Value,
        FileName: entry.FileName,
        MatchedRuleId: entry.MatchedRuleId?.Value,
        MatchedRuleName: entry.MatchedRuleName,
        ActionTaken: entry.ActionTaken,
        DestinationFolderId: entry.DestinationFolderId?.Value,
        AppliedTagNames: entry.AppliedTagNames,
        Success: entry.Success,
        ErrorMessage: entry.ErrorMessage,
        ProcessedAt: entry.ProcessedAt);
}
