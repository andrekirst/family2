using FamilyHub.Api.Features.FileManagement.Application.Queries.GetFavorites;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class GetFavoritesQueryHandlerTests
{
    private static (GetFavoritesQueryHandler handler, FakeUserFavoriteRepository favRepo, FakeStoredFileRepository fileRepo) CreateHandler()
    {
        var favRepo = new FakeUserFavoriteRepository();
        var fileRepo = new FakeStoredFileRepository();
        var handler = new GetFavoritesQueryHandler(favRepo, fileRepo);
        return (handler, favRepo, fileRepo);
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
            UserId.New());
    }

    [Fact]
    public async Task Handle_ShouldReturnFavoritedFiles()
    {
        var userId = UserId.New();
        var familyId = FamilyId.New();
        var (handler, favRepo, fileRepo) = CreateHandler();

        var file1 = CreateTestFile(familyId, "fav1.jpg");
        var file2 = CreateTestFile(familyId, "fav2.jpg");
        fileRepo.Files.Add(file1);
        fileRepo.Files.Add(file2);

        favRepo.Favorites.Add(UserFavorite.Create(userId, file1.Id));
        favRepo.Favorites.Add(UserFavorite.Create(userId, file2.Id));

        var query = new GetFavoritesQuery(userId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyWhenNoFavorites()
    {
        var (handler, _, _) = CreateHandler();

        var query = new GetFavoritesQuery(UserId.New());
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnCurrentUserFavorites()
    {
        var userId = UserId.New();
        var otherUserId = UserId.New();
        var familyId = FamilyId.New();
        var (handler, favRepo, fileRepo) = CreateHandler();

        var myFile = CreateTestFile(familyId, "my-fav.jpg");
        var otherFile = CreateTestFile(familyId, "other-fav.jpg");
        fileRepo.Files.Add(myFile);
        fileRepo.Files.Add(otherFile);

        favRepo.Favorites.Add(UserFavorite.Create(userId, myFile.Id));
        favRepo.Favorites.Add(UserFavorite.Create(otherUserId, otherFile.Id));

        var query = new GetFavoritesQuery(userId);
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("my-fav.jpg");
    }
}
