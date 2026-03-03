using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Photos.Domain.Events;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Photos.Domain.Entities;

public sealed class Photo : AggregateRoot<PhotoId>
{
#pragma warning disable CS8618
    private Photo() { }
#pragma warning restore CS8618

    public static Photo Create(
        FamilyId familyId,
        UserId uploadedBy,
        string fileName,
        string contentType,
        long fileSizeBytes,
        string storagePath,
        PhotoCaption? caption = null)
    {
        var photo = new Photo
        {
            Id = PhotoId.New(),
            FamilyId = familyId,
            UploadedBy = uploadedBy,
            FileName = fileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            StoragePath = storagePath,
            Caption = caption,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        photo.RaiseDomainEvent(new PhotoUploadedEvent(
            photo.Id,
            photo.FamilyId,
            photo.UploadedBy,
            photo.FileName,
            photo.CreatedAt
        ));

        return photo;
    }

    public void UpdateCaption(PhotoCaption? caption)
    {
        if (IsDeleted)
        {
            throw new DomainException("Cannot update a deleted photo", DomainErrorCodes.PhotoAlreadyDeleted);
        }

        Caption = caption;
        UpdatedAt = DateTime.UtcNow;

        if (caption.HasValue)
        {
            RaiseDomainEvent(new PhotoCaptionUpdatedEvent(
                Id,
                caption.Value,
                UpdatedAt
            ));
        }
    }

    public void SoftDelete(UserId deletedBy)
    {
        if (IsDeleted)
        {
            throw new DomainException("Photo is already deleted", DomainErrorCodes.PhotoAlreadyDeleted);
        }

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PhotoDeletedEvent(
            Id,
            FamilyId,
            deletedBy,
            UpdatedAt
        ));
    }

    public FamilyId FamilyId { get; private set; }
    public UserId UploadedBy { get; private set; }
    public string FileName { get; private set; }
    public string ContentType { get; private set; }
    public long FileSizeBytes { get; private set; }
    public string StoragePath { get; private set; }
    public PhotoCaption? Caption { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
}
