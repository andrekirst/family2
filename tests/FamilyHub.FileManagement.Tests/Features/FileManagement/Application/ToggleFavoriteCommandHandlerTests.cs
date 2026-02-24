using FamilyHub.Api.Features.FileManagement.Application.Commands.ToggleFavorite;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class ToggleFavoriteCommandHandlerTests
{
    private static (ToggleFavoriteCommandHandler handler, FakeStoredFileRepository fileRepo, FakeUserFavoriteRepository favRepo) CreateHandler()
    {
        var fileRepo = new FakeStoredFileRepository();
        var favRepo = new FakeUserFavoriteRepository();
        var handler = new ToggleFavoriteCommandHandler(fileRepo, favRepo);
        return (handler, fileRepo, favRepo);
    }

    private static StoredFile CreateTestFile(FamilyId familyId)
    {
        return StoredFile.Create(
            FileName.From("photo.jpg"),
            MimeType.From("image/jpeg"),
            FileSize.From(1024),
            StorageKey.New(),
            Checksum.From("a".PadRight(64, 'a')),
            FolderId.New(),
            familyId,
            UserId.New());
    }

    [Fact]
    public async Task Handle_ShouldFavoriteFile()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (handler, fileRepo, favRepo) = CreateHandler();

        var file = CreateTestFile(familyId);
        fileRepo.Files.Add(file);

        var command = new ToggleFavoriteCommand(file.Id, userId, familyId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFavorited.Should().BeTrue();
        favRepo.Favorites.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldUnfavoriteFile()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var (handler, fileRepo, favRepo) = CreateHandler();

        var file = CreateTestFile(familyId);
        fileRepo.Files.Add(file);

        // Pre-add favorite
        favRepo.Favorites.Add(UserFavorite.Create(userId, file.Id));

        var command = new ToggleFavoriteCommand(file.Id, userId, familyId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFavorited.Should().BeFalse();
        favRepo.Favorites.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileNotFound()
    {
        var (handler, _, _) = CreateHandler();

        var command = new ToggleFavoriteCommand(FileId.New(), UserId.New(), FamilyId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.FileNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileBelongsToDifferentFamily()
    {
        var (handler, fileRepo, _) = CreateHandler();

        var file = CreateTestFile(FamilyId.New());
        fileRepo.Files.Add(file);

        var command = new ToggleFavoriteCommand(file.Id, UserId.New(), FamilyId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
