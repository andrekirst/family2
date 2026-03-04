using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.Photos.Application.Search;
using FamilyHub.Api.Features.Photos.Models;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.Search.Tests;

public class PhotosSearchProviderTests
{
    private static readonly UserId TestUserId = UserId.From(Guid.NewGuid());
    private static readonly FamilyId TestFamilyId = FamilyId.From(Guid.NewGuid());

    [Fact]
    public async Task SearchAsync_MatchesByFileName()
    {
        var photos = new List<PhotoDto>
        {
            CreatePhoto("vacation-beach.jpg", null),
            CreatePhoto("birthday-cake.jpg", null)
        };
        var repo = new FakePhotoRepository(photos);
        var provider = new PhotosSearchProvider(repo);
        var context = new SearchContext(TestUserId, TestFamilyId, "vacation");

        var results = await provider.SearchAsync(context);

        results.Should().HaveCount(1);
        results[0].Title.Should().Be("vacation-beach.jpg");
    }

    [Fact]
    public async Task SearchAsync_MatchesByCaption()
    {
        var photos = new List<PhotoDto>
        {
            CreatePhoto("IMG_001.jpg", "Beautiful sunset at the beach")
        };
        var repo = new FakePhotoRepository(photos);
        var provider = new PhotosSearchProvider(repo);
        var context = new SearchContext(TestUserId, TestFamilyId, "sunset");

        var results = await provider.SearchAsync(context);

        results.Should().HaveCount(1);
        results[0].Title.Should().Be("Beautiful sunset at the beach");
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsEmpty()
    {
        var photos = new List<PhotoDto> { CreatePhoto("test.jpg", null) };
        var repo = new FakePhotoRepository(photos);
        var provider = new PhotosSearchProvider(repo);
        var context = new SearchContext(TestUserId, TestFamilyId, "");

        var results = await provider.SearchAsync(context);

        results.Should().BeEmpty();
    }

    [Fact]
    public void ModuleName_ShouldBePhotos()
    {
        var repo = new FakePhotoRepository();
        var provider = new PhotosSearchProvider(repo);

        provider.ModuleName.Should().Be("photos");
    }

    private static PhotoDto CreatePhoto(string fileName, string? caption) =>
        new()
        {
            Id = Guid.NewGuid(),
            FamilyId = TestFamilyId.Value,
            UploadedBy = TestUserId.Value,
            FileName = fileName,
            ContentType = "image/jpeg",
            FileSizeBytes = 1024,
            StoragePath = $"/photos/{fileName}",
            Caption = caption,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
}
