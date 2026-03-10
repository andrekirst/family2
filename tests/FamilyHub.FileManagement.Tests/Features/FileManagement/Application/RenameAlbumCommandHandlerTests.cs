using FamilyHub.Api.Features.FileManagement.Application.Commands.RenameAlbum;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class RenameAlbumCommandHandlerTests
{
    private readonly IAlbumRepository _albumRepo = Substitute.For<IAlbumRepository>();
    private readonly RenameAlbumCommandHandler _handler;

    public RenameAlbumCommandHandlerTests()
    {
        _handler = new RenameAlbumCommandHandler(_albumRepo, TimeProvider.System);
    }

    [Fact]
    public async Task Handle_ShouldRenameAlbum()
    {
        var familyId = FamilyId.New();
        var album = Album.Create(AlbumName.From("Old Name"), null, familyId, UserId.New(), DateTimeOffset.UtcNow);
        _albumRepo.GetByIdAsync(album.Id, Arg.Any<CancellationToken>()).Returns(album);

        var command = new RenameAlbumCommand(album.Id, AlbumName.From("New Name"))
        {
            FamilyId = familyId,
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.AlbumId.Should().Be(album.Id);
        album.Name.Value.Should().Be("New Name");
    }

    [Fact]
    public async Task Handle_ShouldThrowWhenAlbumNotFound()
    {
        _albumRepo.GetByIdAsync(AlbumId.New(), Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs((Album?)null);

        var command = new RenameAlbumCommand(AlbumId.New(), AlbumName.From("New"))
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

        var command = new RenameAlbumCommand(album.Id, AlbumName.From("New"))
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be(DomainErrorCodes.Forbidden);
    }
}
