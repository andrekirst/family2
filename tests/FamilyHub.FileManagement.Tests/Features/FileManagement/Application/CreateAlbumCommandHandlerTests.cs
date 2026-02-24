using FamilyHub.Api.Features.FileManagement.Application.Commands.CreateAlbum;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class CreateAlbumCommandHandlerTests
{
    private static (CreateAlbumCommandHandler handler, FakeAlbumRepository albumRepo) CreateHandler()
    {
        var albumRepo = new FakeAlbumRepository();
        var handler = new CreateAlbumCommandHandler(albumRepo);
        return (handler, albumRepo);
    }

    [Fact]
    public async Task Handle_ShouldCreateAlbum()
    {
        var (handler, albumRepo) = CreateHandler();

        var command = new CreateAlbumCommand(
            AlbumName.From("Summer 2025"),
            "Beach photos",
            FamilyId.New(),
            UserId.New());

        var result = await handler.Handle(command, CancellationToken.None);

        result.AlbumId.Value.Should().NotBe(Guid.Empty);
        albumRepo.Albums.Should().HaveCount(1);
        albumRepo.Albums.First().Name.Value.Should().Be("Summer 2025");
        albumRepo.Albums.First().Description.Should().Be("Beach photos");
    }

    [Fact]
    public async Task Handle_WithNullDescription_ShouldCreateAlbum()
    {
        var (handler, albumRepo) = CreateHandler();

        var command = new CreateAlbumCommand(
            AlbumName.From("Quick Album"),
            null,
            FamilyId.New(),
            UserId.New());

        var result = await handler.Handle(command, CancellationToken.None);

        result.AlbumId.Value.Should().NotBe(Guid.Empty);
        albumRepo.Albums.First().Description.Should().BeNull();
    }
}
