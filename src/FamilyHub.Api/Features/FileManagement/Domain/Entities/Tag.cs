using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Domain.Entities;

public sealed class Tag : AggregateRoot<TagId>
{
#pragma warning disable CS8618
    private Tag() { }
#pragma warning restore CS8618

    public static Tag Create(
        TagName name,
        TagColor color,
        FamilyId familyId,
        UserId createdBy)
    {
        return new Tag
        {
            Id = TagId.New(),
            Name = name,
            Color = color,
            FamilyId = familyId,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public TagName Name { get; private set; }
    public TagColor Color { get; private set; }
    public FamilyId FamilyId { get; private set; }
    public UserId CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public void Rename(TagName newName)
    {
        Name = newName;
    }

    public void ChangeColor(TagColor newColor)
    {
        Color = newColor;
    }
}
