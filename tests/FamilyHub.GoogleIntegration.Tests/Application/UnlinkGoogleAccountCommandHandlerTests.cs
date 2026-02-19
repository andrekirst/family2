using FluentAssertions;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Application.Commands.UnlinkGoogleAccount;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Events;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;
using FamilyHub.TestCommon.Fakes;

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
            GoogleScopes.From("openid email"));
        link.ClearDomainEvents();
        return link;
    }

    [Fact]
    public async Task Handle_ShouldDeleteLink()
    {
        var userId = UserId.New();
        var existingLink = CreateTestLink(userId);
        var linkRepo = new FakeGoogleAccountLinkRepository(existingLink);
        var oauthService = new FakeGoogleOAuthService();
        var encryptionService = new FakeTokenEncryptionService();
        var handler = new UnlinkGoogleAccountCommandHandler(linkRepo, oauthService, encryptionService);

        var command = new UnlinkGoogleAccountCommand(userId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        linkRepo.DeletedLinks.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldRevokeTokenAtGoogle()
    {
        var userId = UserId.New();
        var existingLink = CreateTestLink(userId);
        var linkRepo = new FakeGoogleAccountLinkRepository(existingLink);
        var oauthService = new FakeGoogleOAuthService();
        var encryptionService = new FakeTokenEncryptionService();
        var handler = new UnlinkGoogleAccountCommandHandler(linkRepo, oauthService, encryptionService);

        var command = new UnlinkGoogleAccountCommand(userId);
        await handler.Handle(command, CancellationToken.None);

        oauthService.TokenRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldRaiseUnlinkedEvent()
    {
        var userId = UserId.New();
        var existingLink = CreateTestLink(userId);
        var linkRepo = new FakeGoogleAccountLinkRepository(existingLink);
        var oauthService = new FakeGoogleOAuthService();
        var encryptionService = new FakeTokenEncryptionService();
        var handler = new UnlinkGoogleAccountCommandHandler(linkRepo, oauthService, encryptionService);

        var command = new UnlinkGoogleAccountCommand(userId);
        await handler.Handle(command, CancellationToken.None);

        existingLink.DomainEvents.Should().HaveCount(1);
        existingLink.DomainEvents.First().Should().BeOfType<GoogleAccountUnlinkedEvent>();
    }

    [Fact]
    public async Task Handle_WhenNoLinkedAccount_ShouldThrow()
    {
        var linkRepo = new FakeGoogleAccountLinkRepository();
        var oauthService = new FakeGoogleOAuthService();
        var encryptionService = new FakeTokenEncryptionService();
        var handler = new UnlinkGoogleAccountCommandHandler(linkRepo, oauthService, encryptionService);

        var command = new UnlinkGoogleAccountCommand(UserId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*No Google account linked*");
    }
}
