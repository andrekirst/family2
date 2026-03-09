using FamilyHub.Api.Features.FileManagement.Application.Commands.CreateShareLink;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class CreateShareLinkCommandHandlerTests
{
    private readonly IShareLinkRepository _repo = Substitute.For<IShareLinkRepository>();
    private readonly CreateShareLinkCommandHandler _handler;

    public CreateShareLinkCommandHandlerTests()
    {
        _handler = new CreateShareLinkCommandHandler(_repo);
    }

    [Fact]
    public async Task Handle_ShouldCreateShareLink()
    {
        var command = new CreateShareLinkCommand(
            ShareResourceType.File,
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(7),
            null,
            null)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        await _repo.Received(1).AddAsync(Arg.Any<ShareLink>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithPassword_ShouldHashPassword()
    {
        var command = new CreateShareLinkCommand(
            ShareResourceType.File,
            Guid.NewGuid(),
            null,
            "my-secret-password",
            null)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _repo.Received(1).AddAsync(
            Arg.Is<ShareLink>(l =>
                l.PasswordHash != null &&
                l.PasswordHash != "my-secret-password" &&
                l.HasPassword),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDownloadLimit_ShouldSetLimit()
    {
        var command = new CreateShareLinkCommand(
            ShareResourceType.Folder,
            Guid.NewGuid(),
            null,
            null,
            10)
        {
            FamilyId = FamilyId.New(),
            UserId = UserId.New()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await _repo.Received(1).AddAsync(
            Arg.Is<ShareLink>(l =>
                l.MaxDownloads == 10 &&
                l.ResourceType == ShareResourceType.Folder),
            Arg.Any<CancellationToken>());
    }
}
