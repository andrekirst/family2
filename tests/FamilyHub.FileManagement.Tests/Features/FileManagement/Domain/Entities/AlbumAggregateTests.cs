using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Events;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Domain.Entities;

public class AlbumAggregateTests
{
    private static Album CreateTestAlbum(FamilyId? familyId = null)
    {
        return Album.Create(
            AlbumName.From("Summer Vacation"),
            "Photos from our trip",
            familyId ?? FamilyId.New(),
            UserId.New());
    }

    [Fact]
    public void Create_ShouldCreateAlbumWithValidData()
    {
        var album = CreateTestAlbum();

        album.Should().NotBeNull();
        album.Id.Value.Should().NotBe(Guid.Empty);
        album.Name.Value.Should().Be("Summer Vacation");
        album.Description.Should().Be("Photos from our trip");
        album.CoverFileId.Should().BeNull();
        album.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_ShouldRaiseAlbumCreatedEvent()
    {
        var album = CreateTestAlbum();

        album.DomainEvents.Should().HaveCount(1);
        var evt = album.DomainEvents.First().Should().BeOfType<AlbumCreatedEvent>().Subject;
        evt.AlbumId.Should().Be(album.Id);
        evt.AlbumName.Should().Be(album.Name);
    }

    [Fact]
    public void Rename_ShouldUpdateName()
    {
        var album = CreateTestAlbum();
        var newName = AlbumName.From("Winter Vacation");

        album.Rename(newName);

        album.Name.Value.Should().Be("Winter Vacation");
    }

    [Fact]
    public void Rename_ShouldUpdateTimestamp()
    {
        var album = CreateTestAlbum();
        var originalUpdatedAt = album.UpdatedAt;

        album.Rename(AlbumName.From("New Name"));

        album.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt);
    }

    [Fact]
    public void UpdateDescription_ShouldChangeDescription()
    {
        var album = CreateTestAlbum();

        album.UpdateDescription("New description");

        album.Description.Should().Be("New description");
    }

    [Fact]
    public void SetCoverImage_ShouldSetCoverFileId()
    {
        var album = CreateTestAlbum();
        var fileId = FileId.New();

        album.SetCoverImage(fileId);

        album.CoverFileId.Should().Be(fileId);
    }

    [Fact]
    public void SetCoverImage_WithNull_ShouldClearCover()
    {
        var album = CreateTestAlbum();
        album.SetCoverImage(FileId.New());

        album.SetCoverImage(null);

        album.CoverFileId.Should().BeNull();
    }
}
