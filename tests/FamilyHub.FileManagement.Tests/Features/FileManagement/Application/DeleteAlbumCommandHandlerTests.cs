using FamilyHub.Api.Features.FileManagement.Application.Commands.DeleteAlbum;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class DeleteAlbumCommandHandlerTests
{
    private readonly IAlbumRepository _albumRepo = Substitute.For<IAlbumRepository>();
    private readonly IAlbumItemRepository _itemRepo = Substitute.For<IAlbumItemRepository>();
    private readonly DeleteAlbumCommandHandler _handler;

    public DeleteAlbumCommandHandlerTests()
    {
        _handler = new DeleteAlbumCommandHandler(_albumRepo, _itemRepo);
    }

    [Fact]
    public async Task Handle_ShouldDeleteAlbum()
    {
        var familyId = FamilyId.New();
        var album = Album.Create(AlbumName.From("Album"), null, familyId, UserId.New(), DateTimeOffset.UtcNow);
        _albumRepo.GetByIdAsync(album.Id, Arg.Any<CancellationToken>()).Returns(album);

        var command = new DeleteAlbumCommand(album.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _albumRepo.Received(1).RemoveAsync(album, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRemoveAlbumItems()
    {
        var familyId = FamilyId.New();
        var album = Album.Create(AlbumName.From("Album"), null, familyId, UserId.New(), DateTimeOffset.UtcNow);
        _albumRepo.GetByIdAsync(album.Id, Arg.Any<CancellationToken>()).Returns(album);

        var command = new DeleteAlbumCommand(album.Id)
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        await _handler.Handle(command, CancellationToken.None);

        await _itemRepo.Received(1).RemoveByAlbumIdAsync(album.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenAlbumNotFound()
    {
        _albumRepo.GetByIdAsync(AlbumId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((Album?)null);

        var command = new DeleteAlbumCommand(AlbumId.New())
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.AlbumNotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenAlbumBelongsToDifferentFamily()
    {
        var album = Album.Create(AlbumName.From("Album"), null, FamilyId.New(), UserId.New(), DateTimeOffset.UtcNow);
        _albumRepo.GetByIdAsync(album.Id, Arg.Any<CancellationToken>()).Returns(album);

        var command = new DeleteAlbumCommand(album.Id)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.Forbidden);
    }
}
