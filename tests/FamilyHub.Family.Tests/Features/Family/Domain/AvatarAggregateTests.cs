using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Common.Infrastructure.Avatar;
using FluentAssertions;

namespace FamilyHub.Family.Tests.Features.Family.Domain;

public class AvatarAggregateTests
{
    [Fact]
    public void Create_ShouldCreateAvatarWithAllVariants()
    {
        // Arrange
        var variants = CreateTestVariantData();

        // Act
        var avatar = AvatarAggregate.Create("photo.jpg", "image/jpeg", variants);

        // Assert
        avatar.Should().NotBeNull();
        avatar.Id.Value.Should().NotBe(Guid.Empty);
        avatar.OriginalFileName.Should().Be("photo.jpg");
        avatar.OriginalMimeType.Should().Be("image/jpeg");
        avatar.Variants.Should().HaveCount(4);
        avatar.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldGenerateUniqueIds()
    {
        // Arrange
        var variants = CreateTestVariantData();

        // Act
        var avatar1 = AvatarAggregate.Create("a.jpg", "image/jpeg", variants);
        var avatar2 = AvatarAggregate.Create("b.jpg", "image/jpeg", variants);

        // Assert
        avatar1.Id.Should().NotBe(avatar2.Id);
    }

    [Fact]
    public void GetVariant_ShouldReturnCorrectVariant()
    {
        // Arrange
        var variants = CreateTestVariantData();
        var avatar = AvatarAggregate.Create("photo.jpg", "image/jpeg", variants);

        // Act
        var tinyVariant = avatar.GetVariant(AvatarSize.Tiny);
        var largeVariant = avatar.GetVariant(AvatarSize.Large);

        // Assert
        tinyVariant.Should().NotBeNull();
        tinyVariant!.Size.Should().Be(AvatarSize.Tiny);
        tinyVariant.StorageKey.Should().Be("key-tiny");
        tinyVariant.Width.Should().Be(24);
        tinyVariant.Height.Should().Be(24);

        largeVariant.Should().NotBeNull();
        largeVariant!.Size.Should().Be(AvatarSize.Large);
        largeVariant.StorageKey.Should().Be("key-large");
        largeVariant.Width.Should().Be(512);
        largeVariant.Height.Should().Be(512);
    }

    [Fact]
    public void Variants_ShouldContainCorrectMetadata()
    {
        // Arrange
        var variants = CreateTestVariantData();
        var avatar = AvatarAggregate.Create("photo.jpg", "image/jpeg", variants);

        // Assert
        foreach (var variant in avatar.Variants)
        {
            variant.AvatarId.Should().Be(avatar.Id);
            variant.MimeType.Should().Be("image/jpeg");
            variant.FileSize.Should().BeGreaterThan(0);
        }
    }

    private static Dictionary<AvatarSize, AvatarVariantData> CreateTestVariantData() => new()
    {
        [AvatarSize.Tiny] = new("key-tiny", "image/jpeg", 100, 24, 24),
        [AvatarSize.Small] = new("key-small", "image/jpeg", 200, 48, 48),
        [AvatarSize.Medium] = new("key-medium", "image/jpeg", 500, 128, 128),
        [AvatarSize.Large] = new("key-large", "image/jpeg", 1000, 512, 512),
    };
}
