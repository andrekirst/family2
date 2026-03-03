using FluentAssertions;
using FamilyHub.Api.Features.Photos.Domain.Entities;
using FamilyHub.Api.Features.Photos.Domain.Events;
using FamilyHub.Api.Features.Photos.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Photos.Tests.Domain;

public class PhotoAggregateTests
{
    private static readonly FamilyId TestFamilyId = FamilyId.From(Guid.NewGuid());
    private static readonly UserId TestUserId = UserId.From(Guid.NewGuid());

    [Fact]
    public void Create_ShouldCreatePhotoWithCorrectProperties()
    {
        // Arrange & Act
        var photo = Photo.Create(
            TestFamilyId,
            TestUserId,
            "family.jpg",
            "image/jpeg",
            1024,
            "/uploads/family.jpg");

        // Assert
        photo.Id.Value.Should().NotBe(Guid.Empty);
        photo.FamilyId.Should().Be(TestFamilyId);
        photo.UploadedBy.Should().Be(TestUserId);
        photo.FileName.Should().Be("family.jpg");
        photo.ContentType.Should().Be("image/jpeg");
        photo.FileSizeBytes.Should().Be(1024);
        photo.StoragePath.Should().Be("/uploads/family.jpg");
        photo.Caption.Should().BeNull();
        photo.IsDeleted.Should().BeFalse();
        photo.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        photo.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithCaption_ShouldSetCaption()
    {
        // Arrange
        var caption = PhotoCaption.From("Family picnic 2026");

        // Act
        var photo = Photo.Create(
            TestFamilyId,
            TestUserId,
            "picnic.jpg",
            "image/jpeg",
            2048,
            "/uploads/picnic.jpg",
            caption);

        // Assert
        photo.Caption.Should().Be(caption);
    }

    [Fact]
    public void Create_ShouldRaisePhotoUploadedEvent()
    {
        // Act
        var photo = Photo.Create(
            TestFamilyId,
            TestUserId,
            "sunset.jpg",
            "image/jpeg",
            4096,
            "/uploads/sunset.jpg");

        // Assert
        photo.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PhotoUploadedEvent>()
            .Which.Should().Match<PhotoUploadedEvent>(e =>
                e.PhotoId == photo.Id &&
                e.FamilyId == TestFamilyId &&
                e.UploadedBy == TestUserId &&
                e.FileName == "sunset.jpg");
    }

    [Fact]
    public void UpdateCaption_ShouldUpdateCaptionAndTimestamp()
    {
        // Arrange
        var photo = Photo.Create(
            TestFamilyId, TestUserId, "photo.jpg", "image/jpeg", 1024, "/uploads/photo.jpg");
        photo.ClearDomainEvents();
        var newCaption = PhotoCaption.From("Updated caption");

        // Act
        photo.UpdateCaption(newCaption);

        // Assert
        photo.Caption.Should().Be(newCaption);
        photo.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateCaption_ShouldRaisePhotoCaptionUpdatedEvent()
    {
        // Arrange
        var photo = Photo.Create(
            TestFamilyId, TestUserId, "photo.jpg", "image/jpeg", 1024, "/uploads/photo.jpg");
        photo.ClearDomainEvents();
        var newCaption = PhotoCaption.From("New caption");

        // Act
        photo.UpdateCaption(newCaption);

        // Assert
        photo.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PhotoCaptionUpdatedEvent>()
            .Which.NewCaption.Should().Be(newCaption);
    }

    [Fact]
    public void UpdateCaption_WhenDeleted_ShouldThrowDomainException()
    {
        // Arrange
        var photo = Photo.Create(
            TestFamilyId, TestUserId, "photo.jpg", "image/jpeg", 1024, "/uploads/photo.jpg");
        photo.SoftDelete(TestUserId);

        // Act & Assert
        var act = () => photo.UpdateCaption(PhotoCaption.From("New caption"));
        act.Should().Throw<DomainException>()
            .WithMessage("Cannot update a deleted photo");
    }

    [Fact]
    public void UpdateCaption_WithNull_ShouldClearCaption()
    {
        // Arrange
        var photo = Photo.Create(
            TestFamilyId, TestUserId, "photo.jpg", "image/jpeg", 1024, "/uploads/photo.jpg",
            PhotoCaption.From("Old caption"));
        photo.ClearDomainEvents();

        // Act
        photo.UpdateCaption(null);

        // Assert
        photo.Caption.Should().BeNull();
        photo.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void SoftDelete_ShouldMarkAsDeleted()
    {
        // Arrange
        var photo = Photo.Create(
            TestFamilyId, TestUserId, "photo.jpg", "image/jpeg", 1024, "/uploads/photo.jpg");
        photo.ClearDomainEvents();

        // Act
        photo.SoftDelete(TestUserId);

        // Assert
        photo.IsDeleted.Should().BeTrue();
        photo.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SoftDelete_ShouldRaisePhotoDeletedEvent()
    {
        // Arrange
        var photo = Photo.Create(
            TestFamilyId, TestUserId, "photo.jpg", "image/jpeg", 1024, "/uploads/photo.jpg");
        photo.ClearDomainEvents();

        // Act
        photo.SoftDelete(TestUserId);

        // Assert
        photo.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PhotoDeletedEvent>()
            .Which.Should().Match<PhotoDeletedEvent>(e =>
                e.PhotoId == photo.Id &&
                e.FamilyId == TestFamilyId &&
                e.DeletedBy == TestUserId);
    }

    [Fact]
    public void SoftDelete_WhenAlreadyDeleted_ShouldThrowDomainException()
    {
        // Arrange
        var photo = Photo.Create(
            TestFamilyId, TestUserId, "photo.jpg", "image/jpeg", 1024, "/uploads/photo.jpg");
        photo.SoftDelete(TestUserId);

        // Act & Assert
        var act = () => photo.SoftDelete(TestUserId);
        act.Should().Throw<DomainException>()
            .WithMessage("Photo is already deleted");
    }
}
