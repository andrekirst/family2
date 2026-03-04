using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.FileManagement.Application.Search;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.Search.Tests;

public class FileManagementSearchProviderTests
{
    private static readonly UserId TestUserId = UserId.From(Guid.NewGuid());
    private static readonly FamilyId TestFamilyId = FamilyId.From(Guid.NewGuid());

    [Fact]
    public async Task SearchAsync_MatchesFilesByName()
    {
        var fileRepo = new FakeStoredFileRepository();
        var file = StoredFile.Create(
            FileName.From("vacation-report.pdf"), MimeType.From("application/pdf"),
            FileSize.From(1024), StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            FolderId.New(), TestFamilyId, TestUserId);
        file.ClearDomainEvents();
        fileRepo.Files.Add(file);

        var provider = CreateProvider(fileRepo: fileRepo);
        var context = new SearchContext(TestUserId, TestFamilyId, "vacation");

        var results = await provider.SearchAsync(context);

        results.Should().Contain(r => r.Title == "vacation-report.pdf");
    }

    [Fact]
    public async Task SearchAsync_MatchesAlbumsByName()
    {
        var albumRepo = new FakeAlbumRepository();
        var album = Album.Create(
            AlbumName.From("Summer Vacation"), "Photos from our trip",
            TestFamilyId, TestUserId);
        album.ClearDomainEvents();
        albumRepo.Albums.Add(album);

        var provider = CreateProvider(albumRepo: albumRepo);
        var context = new SearchContext(TestUserId, TestFamilyId, "vacation");

        var results = await provider.SearchAsync(context);

        results.Should().Contain(r => r.Title == "Summer Vacation");
    }

    [Fact]
    public async Task SearchAsync_MatchesTagsByName()
    {
        var tagRepo = new FakeTagRepository();
        var tag = Tag.Create(TagName.From("vacation"), TagColor.From("#FF0000"),
            TestFamilyId, TestUserId);
        tag.ClearDomainEvents();
        tagRepo.Tags.Add(tag);

        var provider = CreateProvider(tagRepo: tagRepo);
        var context = new SearchContext(TestUserId, TestFamilyId, "vacation");

        var results = await provider.SearchAsync(context);

        results.Should().Contain(r => r.Title == "vacation" && r.Icon == "tag");
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsEmpty()
    {
        var provider = CreateProvider();
        var context = new SearchContext(TestUserId, TestFamilyId, "");

        var results = await provider.SearchAsync(context);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_NoFamily_ReturnsEmpty()
    {
        var provider = CreateProvider();
        var context = new SearchContext(TestUserId, null, "test");

        var results = await provider.SearchAsync(context);

        results.Should().BeEmpty();
    }

    [Fact]
    public void ModuleName_ShouldBeFiles()
    {
        var provider = CreateProvider();

        provider.ModuleName.Should().Be("files");
    }

    private static FileManagementSearchProvider CreateProvider(
        FakeStoredFileRepository? fileRepo = null,
        FakeFolderRepository? folderRepo = null,
        FakeAlbumRepository? albumRepo = null,
        FakeTagRepository? tagRepo = null,
        FakeSecureNoteRepository? secureNoteRepo = null,
        FakeShareLinkRepository? shareLinkRepo = null) =>
        new(
            fileRepo ?? new FakeStoredFileRepository(),
            folderRepo ?? new FakeFolderRepository(),
            albumRepo ?? new FakeAlbumRepository(),
            tagRepo ?? new FakeTagRepository(),
            secureNoteRepo ?? new FakeSecureNoteRepository(),
            shareLinkRepo ?? new FakeShareLinkRepository());
}
