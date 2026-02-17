using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Common.Infrastructure.Avatar;

/// <summary>
/// Avatar aggregate root. Owns multiple size variants.
/// Each variant stores metadata and a reference to the stored file.
/// </summary>
public sealed class AvatarAggregate
{
    // Private parameterless constructor for EF Core
#pragma warning disable CS8618
    private AvatarAggregate() { }
#pragma warning restore CS8618

    public AvatarId Id { get; private set; }
    public string OriginalFileName { get; private set; }
    public string OriginalMimeType { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<AvatarVariant> _variants = [];
    public IReadOnlyCollection<AvatarVariant> Variants => _variants.AsReadOnly();

    /// <summary>
    /// Factory method to create a new avatar with all its size variants.
    /// </summary>
    public static AvatarAggregate Create(
        string originalFileName,
        string originalMimeType,
        Dictionary<AvatarSize, AvatarVariantData> variants)
    {
        var avatar = new AvatarAggregate
        {
            Id = AvatarId.New(),
            OriginalFileName = originalFileName,
            OriginalMimeType = originalMimeType,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var (size, data) in variants)
        {
            avatar._variants.Add(AvatarVariant.Create(
                avatar.Id,
                size,
                data.StorageKey,
                data.MimeType,
                data.FileSize,
                data.Width,
                data.Height));
        }

        return avatar;
    }

    /// <summary>
    /// Get a specific size variant.
    /// </summary>
    public AvatarVariant? GetVariant(AvatarSize size) =>
        _variants.FirstOrDefault(v => v.Size == size);
}

/// <summary>
/// Data for creating a single avatar variant.
/// </summary>
public record AvatarVariantData(
    string StorageKey,
    string MimeType,
    int FileSize,
    int Width,
    int Height);
