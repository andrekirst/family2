using FamilyHub.Api.Features.FileManagement.Application.Queries.GetFavorites;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetFavoritesQueryHandlerTests
{
    private readonly IUserFavoriteRepository _favRepo = Substitute.For<IUserFavoriteRepository>();
    private readonly IStoredFileRepository _fileRepo = Substitute.For<IStoredFileRepository>();
    private readonly GetFavoritesQueryHandler _handler;

    public GetFavoritesQueryHandlerTests()
    {
        _handler = new GetFavoritesQueryHandler(_favRepo, _fileRepo);
    }

    private static StoredFile CreateTestFile(FamilyId familyId, string name = "photo.jpg")
    {
        return StoredFile.Create(
            FileName.From(name),
            MimeType.From("image/jpeg"),
            FileSize.From(1024),
            StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            FolderId.New(),
            familyId,
            UserId.New(), DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Handle_ShouldReturnFavoritedFiles()
    {
        var userId = UserId.New();
        var familyId = FamilyId.New();

        var file1 = CreateTestFile(familyId, "fav1.jpg");
        var file2 = CreateTestFile(familyId, "fav2.jpg");

        var favs = new List<UserFavorite>
        {
            UserFavorite.Create(userId, file1.Id, DateTimeOffset.UtcNow),
            UserFavorite.Create(userId, file2.Id, DateTimeOffset.UtcNow)
        };
        _favRepo.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(favs);
        _fileRepo.GetByIdsAsync(
            Arg.Is<List<FileId>>(ids => ids.Count == 2),
            Arg.Any<CancellationToken>())
            .Returns(new List<StoredFile> { file1, file2 });

        var query = new GetFavoritesQuery()
        {
            UserId = userId,
            FamilyId = familyId
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyWhenNoFavorites()
    {
        var userId = UserId.New();
        _favRepo.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<UserFavorite>());

        var query = new GetFavoritesQuery()
        {
            UserId = userId,
            FamilyId = FamilyId.New()
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnCurrentUserFavorites()
    {
        var userId = UserId.New();
        var familyId = FamilyId.New();

        var myFile = CreateTestFile(familyId, "my-fav.jpg");

        var favs = new List<UserFavorite> { UserFavorite.Create(userId, myFile.Id, DateTimeOffset.UtcNow) };
        _favRepo.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(favs);
        _fileRepo.GetByIdsAsync(
            Arg.Any<List<FileId>>(),
            Arg.Any<CancellationToken>())
            .Returns(new List<StoredFile> { myFile });

        var query = new GetFavoritesQuery()
        {
            UserId = userId,
            FamilyId = familyId
        };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("my-fav.jpg");
    }
}
