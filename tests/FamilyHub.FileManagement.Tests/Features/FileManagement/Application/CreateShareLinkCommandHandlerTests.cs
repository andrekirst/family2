using FamilyHub.Api.Features.FileManagement.Application.Commands.CreateShareLink;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;
using FluentAssertions;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class CreateShareLinkCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateShareLink()
    {
        var repo = new FakeShareLinkRepository();
        var handler = new CreateShareLinkCommandHandler(repo);

        var command = new CreateShareLinkCommand(
            ShareResourceType.File,
            Guid.NewGuid(),
            FamilyId.New(),
            UserId.New(),
            DateTime.UtcNow.AddDays(7),
            null,
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        repo.Links.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithPassword_ShouldHashPassword()
    {
        var repo = new FakeShareLinkRepository();
        var handler = new CreateShareLinkCommandHandler(repo);

        var command = new CreateShareLinkCommand(
            ShareResourceType.File,
            Guid.NewGuid(),
            FamilyId.New(),
            UserId.New(),
            null,
            "my-secret-password",
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        repo.Links.First().PasswordHash.Should().NotBeNull();
        repo.Links.First().PasswordHash.Should().NotBe("my-secret-password"); // Should be hashed
        repo.Links.First().HasPassword.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithDownloadLimit_ShouldSetLimit()
    {
        var repo = new FakeShareLinkRepository();
        var handler = new CreateShareLinkCommandHandler(repo);

        var command = new CreateShareLinkCommand(
            ShareResourceType.Folder,
            Guid.NewGuid(),
            FamilyId.New(),
            UserId.New(),
            null,
            null,
            10);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        repo.Links.First().MaxDownloads.Should().Be(10);
        repo.Links.First().ResourceType.Should().Be(ShareResourceType.Folder);
    }
}
