using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain;

public class FileThumbnailTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var fileId = FileId.New();
        var storageKey = StorageKey.From("thumbnails/abc/200x200.webp");

        var thumbnail = FileThumbnail.Create(fileId, 200, 200, storageKey);

        thumbnail.FileId.Should().Be(fileId);
        thumbnail.Width.Should().Be(200);
        thumbnail.Height.Should().Be(200);
        thumbnail.StorageKey.Should().Be(storageKey);
        thumbnail.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ShouldRaiseThumbnailGeneratedEvent()
    {
        var fileId = FileId.New();
        var storageKey = StorageKey.From("thumbnails/abc/800x800.webp");

        var thumbnail = FileThumbnail.Create(fileId, 800, 800, storageKey);

        var domainEvent = thumbnail.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ThumbnailGeneratedEvent>().Subject;
        domainEvent.FileId.Should().Be(fileId);
        domainEvent.ThumbnailStorageKey.Should().Be(storageKey);
        domainEvent.Width.Should().Be(800);
        domainEvent.Height.Should().Be(800);
    }

    [Fact]
    public void Create_ShouldGenerateUniqueId()
    {
        var fileId = FileId.New();
        var storageKey = StorageKey.From("thumbnails/abc/200x200.webp");

        var thumb1 = FileThumbnail.Create(fileId, 200, 200, storageKey);
        var thumb2 = FileThumbnail.Create(fileId, 800, 800, storageKey);

        thumb1.Id.Should().NotBe(thumb2.Id);
    }
}
