using FamilyHub.Api.Features.Photos.Domain.Entities;
using FamilyHub.Api.Features.Photos.Models;

namespace FamilyHub.Api.Features.Photos.Application.Mappers;

public static class PhotoMapper
{
    public static PhotoDto ToDto(Photo photo)
    {
        return new PhotoDto
        {
            Id = photo.Id.Value,
            FamilyId = photo.FamilyId.Value,
            UploadedBy = photo.UploadedBy.Value,
            FileName = photo.FileName,
            ContentType = photo.ContentType,
            FileSizeBytes = photo.FileSizeBytes,
            StoragePath = photo.StoragePath,
            Caption = photo.Caption?.Value,
            CreatedAt = photo.CreatedAt,
            UpdatedAt = photo.UpdatedAt
        };
    }
}
