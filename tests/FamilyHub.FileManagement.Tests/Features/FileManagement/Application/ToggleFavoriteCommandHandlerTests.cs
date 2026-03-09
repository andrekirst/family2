using FamilyHub.Api.Features.FileManagement.Application.Commands.ToggleFavorite;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class ToggleFavoriteCommandHandlerTests
{
    private readonly IStoredFileRepository _fileRepo = Substitute.For<IStoredFileRepository>();
    private readonly IUserFavoriteRepository _favRepo = Substitute.For<IUserFavoriteRepository>();
    private readonly ToggleFavoriteCommandHandler _handler;

    public ToggleFavoriteCommandHandlerTests()
    {
        _handler = new ToggleFavoriteCommandHandler(_fileRepo, _favRepo, TimeProvider.System);
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
            UserId.New(), DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Handle_ShouldFavoriteFile()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var file = CreateTestFile(familyId);

        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _favRepo.ExistsAsync(userId, file.Id, Arg.Any<CancellationToken>()).Returns(false);

        var command = new ToggleFavoriteCommand(file.Id)
        {
            UserId = userId,
            FamilyId = familyId
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFavorited.Should().BeTrue();
        await _favRepo.Received(1).AddAsync(Arg.Any<UserFavorite>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUnfavoriteFile()
    {
        var familyId = FamilyId.New();
        var userId = UserId.New();
        var file = CreateTestFile(familyId);
        var favorite = UserFavorite.Create(userId, file.Id, DateTimeOffset.UtcNow);

        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
        _favRepo.ExistsAsync(userId, file.Id, Arg.Any<CancellationToken>()).Returns(true);
        _favRepo.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<UserFavorite> { favorite });

        var command = new ToggleFavoriteCommand(file.Id)
        {
            UserId = userId,
            FamilyId = familyId
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFavorited.Should().BeFalse();
        await _favRepo.Received(1).RemoveAsync(Arg.Any<UserFavorite>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileNotFound()
    {
        _fileRepo.GetByIdAsync(FileId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((StoredFile?)null);

        var command = new ToggleFavoriteCommand(FileId.New())
        {
            UserId = UserId.New(),
            FamilyId = FamilyId.New()
        };
        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.FileNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenFileBelongsToDifferentFamily()
    {
        var file = CreateTestFile(FamilyId.New());

        _fileRepo.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

        var command = new ToggleFavoriteCommand(file.Id)
        {
            UserId = UserId.New(),
            FamilyId = FamilyId.New()
        };
        var act = () => _handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == DomainErrorCodes.Forbidden);
    }
}
