using FamilyHub.Api.Features.FileManagement.Application.Commands.AccessShareLink;
using FamilyHub.Api.Features.FileManagement.Domain.Entities;
using FamilyHub.Api.Features.FileManagement.Domain.Repositories;
using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace FamilyHub.FileManagement.Tests.Features.FileManagement.Application;

public class AccessShareLinkCommandHandlerTests
{
    [Fact]
    public async Task Handle_View_ShouldSucceedAndLogAccess()
    {
        var linkRepo = Substitute.For<IShareLinkRepository>();
        var logRepo = Substitute.For<IShareLinkAccessLogRepository>();
        var handler = new AccessShareLinkCommandHandler(linkRepo, logRepo);

        var link = ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(), DateTime.UtcNow.AddDays(1), null, null);
        linkRepo.GetByTokenAsync(link.Token, Arg.Any<CancellationToken>()).Returns(link);

        var command = new AccessShareLinkCommand(link.Token, null, "192.168.1.1", "Mozilla/5.0", ShareAccessAction.View);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.ResourceType.Should().Be("File");
        await logRepo.Received(1).AddAsync(
            Arg.Is<ShareLinkAccessLog>(l => l.IpAddress == "192.168.1.1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Download_ShouldIncrementDownloadCount()
    {
        var linkRepo = Substitute.For<IShareLinkRepository>();
        var logRepo = Substitute.For<IShareLinkAccessLogRepository>();
        var handler = new AccessShareLinkCommandHandler(linkRepo, logRepo);

        var link = ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(), null, null, 5);
        linkRepo.GetByTokenAsync(link.Token, Arg.Any<CancellationToken>()).Returns(link);

        var command = new AccessShareLinkCommand(link.Token, null, "10.0.0.1", null, ShareAccessAction.Download);
        await handler.Handle(command, CancellationToken.None);

        link.DownloadCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithPassword_CorrectPassword_ShouldSucceed()
    {
        var linkRepo = Substitute.For<IShareLinkRepository>();
        var logRepo = Substitute.For<IShareLinkAccessLogRepository>();
        var handler = new AccessShareLinkCommandHandler(linkRepo, logRepo);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("secret123");
        var link = ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(), null, passwordHash, null);
        linkRepo.GetByTokenAsync(link.Token, Arg.Any<CancellationToken>()).Returns(link);

        var command = new AccessShareLinkCommand(link.Token, "secret123", "10.0.0.1", null, ShareAccessAction.View);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithPassword_WrongPassword_ShouldThrow()
    {
        var linkRepo = Substitute.For<IShareLinkRepository>();
        var logRepo = Substitute.For<IShareLinkAccessLogRepository>();
        var handler = new AccessShareLinkCommandHandler(linkRepo, logRepo);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("secret123");
        var link = ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(), null, passwordHash, null);
        linkRepo.GetByTokenAsync(link.Token, Arg.Any<CancellationToken>()).Returns(link);

        var command = new AccessShareLinkCommand(link.Token, "wrongpassword", "10.0.0.1", null, ShareAccessAction.View);
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Incorrect password*");
    }

    [Fact]
    public async Task Handle_WithPassword_NoPasswordProvided_ShouldThrow()
    {
        var linkRepo = Substitute.For<IShareLinkRepository>();
        var logRepo = Substitute.For<IShareLinkAccessLogRepository>();
        var handler = new AccessShareLinkCommandHandler(linkRepo, logRepo);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("secret123");
        var link = ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(), null, passwordHash, null);
        linkRepo.GetByTokenAsync(link.Token, Arg.Any<CancellationToken>()).Returns(link);

        var command = new AccessShareLinkCommand(link.Token, null, "10.0.0.1", null, ShareAccessAction.View);
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Password required*");
    }

    [Fact]
    public async Task Handle_Revoked_ShouldThrow()
    {
        var linkRepo = Substitute.For<IShareLinkRepository>();
        var logRepo = Substitute.For<IShareLinkAccessLogRepository>();
        var handler = new AccessShareLinkCommandHandler(linkRepo, logRepo);

        var link = ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(), null, null, null);
        link.Revoke(UserId.New());
        linkRepo.GetByTokenAsync(link.Token, Arg.Any<CancellationToken>()).Returns(link);

        var command = new AccessShareLinkCommand(link.Token, null, "10.0.0.1", null, ShareAccessAction.View);
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*revoked*");
    }

    [Fact]
    public async Task Handle_Expired_ShouldThrow()
    {
        var linkRepo = Substitute.For<IShareLinkRepository>();
        var logRepo = Substitute.For<IShareLinkAccessLogRepository>();
        var handler = new AccessShareLinkCommandHandler(linkRepo, logRepo);

        var link = ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(), DateTime.UtcNow.AddHours(-1), null, null);
        linkRepo.GetByTokenAsync(link.Token, Arg.Any<CancellationToken>()).Returns(link);

        var command = new AccessShareLinkCommand(link.Token, null, "10.0.0.1", null, ShareAccessAction.View);
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task Handle_DownloadLimitReached_ShouldThrow()
    {
        var linkRepo = Substitute.For<IShareLinkRepository>();
        var logRepo = Substitute.For<IShareLinkAccessLogRepository>();
        var handler = new AccessShareLinkCommandHandler(linkRepo, logRepo);

        var link = ShareLink.Create(ShareResourceType.File, Guid.NewGuid(), FamilyId.New(), UserId.New(), null, null, 1);
        link.IncrementDownloadCount(); // Already at limit
        linkRepo.GetByTokenAsync(link.Token, Arg.Any<CancellationToken>()).Returns(link);

        var command = new AccessShareLinkCommand(link.Token, null, "10.0.0.1", null, ShareAccessAction.Download);
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Download limit reached*");
    }

    [Fact]
    public async Task Handle_TokenNotFound_ShouldThrow()
    {
        var linkRepo = Substitute.For<IShareLinkRepository>();
        var logRepo = Substitute.For<IShareLinkAccessLogRepository>();
        var handler = new AccessShareLinkCommandHandler(linkRepo, logRepo);

        linkRepo.GetByTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ShareLink?)null);

        var command = new AccessShareLinkCommand("nonexistent-token", null, "10.0.0.1", null, ShareAccessAction.View);
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Share link not found*");
    }
}
