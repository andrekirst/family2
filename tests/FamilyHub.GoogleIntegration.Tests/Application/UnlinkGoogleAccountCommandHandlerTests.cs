using FluentAssertions;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Application.Commands.UnlinkGoogleAccount;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Events;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Services;
using NSubstitute;

namespace FamilyHub.GoogleIntegration.Tests.Application;

public class UnlinkGoogleAccountCommandHandlerTests
{
    private static GoogleAccountLink CreateTestLink(UserId userId)
    {
        var link = GoogleAccountLink.Create(
            userId,
            GoogleAccountId.From("google-sub"),
            Email.From("test@gmail.com"),
            EncryptedToken.From("encrypted:access-token"),
            EncryptedToken.From("encrypted:refresh-token"),
            DateTime.UtcNow.AddHours(1),
            GoogleScopes.From("openid email"), DateTimeOffset.UtcNow);
        link.ClearDomainEvents();
        return link;
    }

    private static (UnlinkGoogleAccountCommandHandler Handler,
        IGoogleAccountLinkRepository LinkRepo,
        IGoogleOAuthService OAuthService) CreateHandler(
        UserId userId, GoogleAccountLink existingLink)
    {
        var linkRepo = Substitute.For<IGoogleAccountLinkRepository>();
        linkRepo.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(existingLink);

        var oauthService = Substitute.For<IGoogleOAuthService>();

        var encryptionService = Substitute.For<ITokenEncryptionService>();
        encryptionService.Decrypt(Arg.Any<string>())
            .Returns(callInfo => callInfo.ArgAt<string>(0).Replace("encrypted:", ""));

        var handler = new UnlinkGoogleAccountCommandHandler(linkRepo, oauthService, encryptionService);
        return (handler, linkRepo, oauthService);
    }

    [Fact]
    public async Task Handle_ShouldDeleteLink()
    {
        var userId = UserId.New();
        var existingLink = CreateTestLink(userId);
        var (handler, linkRepo, _) = CreateHandler(userId, existingLink);

        var command = new UnlinkGoogleAccountCommand { UserId = userId };
        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        await linkRepo.Received(1).DeleteAsync(existingLink, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRevokeTokenAtGoogle()
    {
        var userId = UserId.New();
        var existingLink = CreateTestLink(userId);
        var (handler, _, oauthService) = CreateHandler(userId, existingLink);

        var command = new UnlinkGoogleAccountCommand { UserId = userId };
        await handler.Handle(command, CancellationToken.None);

        await oauthService.Received(1).RevokeTokenAsync(
            "access-token", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRaiseUnlinkedEvent()
    {
        var userId = UserId.New();
        var existingLink = CreateTestLink(userId);
        var (handler, _, _) = CreateHandler(userId, existingLink);

        var command = new UnlinkGoogleAccountCommand { UserId = userId };
        await handler.Handle(command, CancellationToken.None);

        existingLink.DomainEvents.Should().HaveCount(1);
        existingLink.DomainEvents.First().Should().BeOfType<GoogleAccountUnlinkedEvent>();
    }
}
