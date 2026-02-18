using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain;

public class FileVersionTests
{
    private static readonly string ValidChecksum = new('a', 64);

    [Fact]
    public void Create_ShouldSetProperties()
    {
        var fileId = FileId.New();
        var storageKey = StorageKey.From("versions/test-key");
        var fileSize = FileSize.From(1024);
        var checksum = Checksum.From(ValidChecksum);
        var uploadedBy = UserId.New();

        var version = FileVersion.Create(fileId, 1, storageKey, fileSize, checksum, uploadedBy);

        version.FileId.Should().Be(fileId);
        version.VersionNumber.Should().Be(1);
        version.StorageKey.Should().Be(storageKey);
        version.FileSize.Should().Be(fileSize);
        version.Checksum.Should().Be(checksum);
        version.UploadedBy.Should().Be(uploadedBy);
        version.IsCurrent.Should().BeTrue();
        version.UploadedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithIsCurrentFalse_ShouldNotBeCurrent()
    {
        var version = FileVersion.Create(
            FileId.New(), 2,
            StorageKey.From("key"),
            FileSize.From(100),
            Checksum.From(ValidChecksum),
            UserId.New(),
            isCurrent: false);

        version.IsCurrent.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        var version = FileVersion.Create(
            FileId.New(), 1,
            StorageKey.From("key"),
            FileSize.From(500),
            Checksum.From(ValidChecksum),
            UserId.New());

        version.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void MarkAsNotCurrent_ShouldSetIsCurrentFalse()
    {
        var version = FileVersion.Create(
            FileId.New(), 1,
            StorageKey.From("key"),
            FileSize.From(100),
            Checksum.From(ValidChecksum),
            UserId.New());

        version.MarkAsNotCurrent();

        version.IsCurrent.Should().BeFalse();
    }

    [Fact]
    public void MarkAsCurrent_ShouldSetIsCurrentTrue()
    {
        var version = FileVersion.Create(
            FileId.New(), 1,
            StorageKey.From("key"),
            FileSize.From(100),
            Checksum.From(ValidChecksum),
            UserId.New(),
            isCurrent: false);

        version.MarkAsCurrent();

        version.IsCurrent.Should().BeTrue();
    }
}
