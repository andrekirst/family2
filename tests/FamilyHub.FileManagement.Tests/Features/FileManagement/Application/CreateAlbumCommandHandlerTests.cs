using FamilyHub.Api.Features.FileManagement.Application.Commands.CreateAlbum;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class CreateAlbumCommandHandlerTests
{
    private readonly IAlbumRepository _albumRepo = Substitute.For<IAlbumRepository>();
    private readonly CreateAlbumCommandHandler _handler;

    public CreateAlbumCommandHandlerTests()
    {
        _handler = new CreateAlbumCommandHandler(_albumRepo);
    }

    [Fact]
    public async Task Handle_ShouldCreateAlbum()
    {
        var command = new CreateAlbumCommand(
            AlbumName.From("Summer 2025"),
            "Beach photos")
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AlbumId.Value.Should().NotBe(Guid.Empty);
        await _albumRepo.Received(1).AddAsync(
            Arg.Is<Album>(a => a.Name.Value == "Summer 2025" && a.Description == "Beach photos"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNullDescription_ShouldCreateAlbum()
    {
        var command = new CreateAlbumCommand(
            AlbumName.From("Quick Album"),
            null)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AlbumId.Value.Should().NotBe(Guid.Empty);
        await _albumRepo.Received(1).AddAsync(
            Arg.Is<Album>(a => a.Description == null),
            Arg.Any<CancellationToken>());
    }
}
