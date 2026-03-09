using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.FileManagement.Application.Search;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using SecureNote = FamilyHub.Api.Features.FileManagement.Domain.Entities.SecureNote;

namespace FamilyHub.Search.Tests;

public class FileManagementSearchProviderTests
{
    private static readonly UserId TestUserId = UserId.From(Guid.NewGuid());
    private static readonly FamilyId TestFamilyId = FamilyId.From(Guid.NewGuid());

    [Fact]
    public async Task SearchAsync_MatchesFilesByName()
    {
        var fileRepo = Substitute.For<IStoredFileRepository>();
        var file = StoredFile.Create(
            FileName.From("vacation-report.pdf"), MimeType.From("application/pdf"),
            FileSize.From(1024), StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            FolderId.New(), TestFamilyId, TestUserId);
        file.ClearDomainEvents();

        fileRepo.GetByFamilyIdAsync(TestFamilyId, Arg.Any<CancellationToken>())
            .Returns(new List<StoredFile> { file });

        var provider = CreateProvider(fileRepo: fileRepo);
        var context = new SearchContext(TestUserId, TestFamilyId, "vacation");

        var results = await provider.SearchAsync(context);

        results.Should().Contain(r => r.Title == "vacation-report.pdf");
    }

    [Fact]
    public async Task SearchAsync_MatchesAlbumsByName()
    {
        var albumRepo = Substitute.For<IAlbumRepository>();
        var album = Album.Create(
            AlbumName.From("Summer Vacation"), "Photos from our trip",
            TestFamilyId, TestUserId);
        album.ClearDomainEvents();

        albumRepo.GetByFamilyIdAsync(TestFamilyId, Arg.Any<CancellationToken>())
            .Returns(new List<Album> { album });

        var provider = CreateProvider(albumRepo: albumRepo);
        var context = new SearchContext(TestUserId, TestFamilyId, "vacation");

        var results = await provider.SearchAsync(context);

        results.Should().Contain(r => r.Title == "Summer Vacation");
    }

    [Fact]
    public async Task SearchAsync_MatchesTagsByName()
    {
        var tagRepo = Substitute.For<ITagRepository>();
        var tag = Tag.Create(TagName.From("vacation"), TagColor.From("#FF0000"),
            TestFamilyId, TestUserId);
        tag.ClearDomainEvents();

        tagRepo.GetByFamilyIdAsync(TestFamilyId, Arg.Any<CancellationToken>())
            .Returns(new List<Tag> { tag });

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
        IStoredFileRepository? fileRepo = null,
        IFolderRepository? folderRepo = null,
        IAlbumRepository? albumRepo = null,
        ITagRepository? tagRepo = null,
        ISecureNoteRepository? secureNoteRepo = null,
        IShareLinkRepository? shareLinkRepo = null)
    {
        if (fileRepo is null)
        {
            fileRepo = Substitute.For<IStoredFileRepository>();
            fileRepo.GetByFamilyIdAsync(TestFamilyId, Arg.Any<CancellationToken>())
                .Returns(new List<StoredFile>());
        }

        if (albumRepo is null)
        {
            albumRepo = Substitute.For<IAlbumRepository>();
            albumRepo.GetByFamilyIdAsync(TestFamilyId, Arg.Any<CancellationToken>())
                .Returns(new List<Album>());
        }

        if (tagRepo is null)
        {
            tagRepo = Substitute.For<ITagRepository>();
            tagRepo.GetByFamilyIdAsync(TestFamilyId, Arg.Any<CancellationToken>())
                .Returns(new List<Tag>());
        }

        if (secureNoteRepo is null)
        {
            secureNoteRepo = Substitute.For<ISecureNoteRepository>();
            secureNoteRepo.GetByUserIdAsync(TestUserId, TestFamilyId, Arg.Any<CancellationToken>())
                .Returns(new List<SecureNote>());
        }

        if (shareLinkRepo is null)
        {
            shareLinkRepo = Substitute.For<IShareLinkRepository>();
            shareLinkRepo.GetByFamilyIdAsync(TestFamilyId, Arg.Any<CancellationToken>())
                .Returns(new List<ShareLink>());
        }

        return new FileManagementSearchProvider(
            fileRepo,
            folderRepo ?? Substitute.For<IFolderRepository>(),
            albumRepo,
            tagRepo,
            secureNoteRepo,
            shareLinkRepo);
    }
}
