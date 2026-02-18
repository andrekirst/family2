using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

public sealed class Album : AggregateRoot<AlbumId>
{
#pragma warning disable CS8618
    private Album() { }
#pragma warning restore CS8618

    public static Album Create(
        AlbumName name,
        string? description,
        FamilyId familyId,
        UserId createdBy)
    {
        var album = new Album
        {
            Id = AlbumId.New(),
            Name = name,
            Description = description,
            CoverFileId = null,
            FamilyId = familyId,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        album.RaiseDomainEvent(new AlbumCreatedEvent(
            album.Id, album.Name, album.FamilyId, createdBy, album.CreatedAt));

        return album;
    }

    public AlbumName Name { get; private set; }
    public string? Description { get; private set; }
    public FileId? CoverFileId { get; private set; }
    public FamilyId FamilyId { get; private set; }
    public UserId CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void Rename(AlbumName newName)
    {
        Name = newName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCoverImage(FileId? fileId)
    {
        CoverFileId = fileId;
        UpdatedAt = DateTime.UtcNow;
    }
}
