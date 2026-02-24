using FluentAssertions;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Application.Commands.LinkGoogleAccount;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Events;
using FamilyHub.TestCommon.Fakes;

namespace FamilyHub.GoogleIntegration.Tests.Application;

public class LinkGoogleAccountCommandHandlerTests
{
    private static (LinkGoogleAccountCommandHandler Handler,
        FakeGoogleAccountLinkRepository LinkRepo,
        FakeOAuthStateRepository StateRepo,
        FakeGoogleOAuthService OAuthService,
        FakeTokenEncryptionService EncryptionService)
        CreateHandler(OAuthState? existingState = null)
    {
        var stateRepo = new FakeOAuthStateRepository(existingState);
        var linkRepo = new FakeGoogleAccountLinkRepository();
        var oauthService = new FakeGoogleOAuthService();
        var encryptionService = new FakeTokenEncryptionService();
        var handler = new LinkGoogleAccountCommandHandler(
            stateRepo, linkRepo, oauthService, encryptionService);

        return (handler, linkRepo, stateRepo, oauthService, encryptionService);
    }

    [Fact]
    public async Task Handle_WithValidState_ShouldCreateLink()
    {
        var userId = UserId.New();
        var state = OAuthState.Create("valid-state", userId, "code-verifier");
        var (handler, linkRepo, _, _, _) = CreateHandler(state);

        var command = new LinkGoogleAccountCommand("auth-code", "valid-state");
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        linkRepo.AddedLinks.Should().HaveCount(1);

        var link = linkRepo.AddedLinks.First();
        link.UserId.Should().Be(userId);
        link.GoogleAccountId.Value.Should().Be("google-user-123");
        link.GoogleEmail.Value.Should().Be("test@gmail.com");
    }

    [Fact]
    public async Task Handle_WithValidState_ShouldEncryptTokens()
    {
        var userId = UserId.New();
        var state = OAuthState.Create("state", userId, "verifier");
        var (handler, linkRepo, _, _, _) = CreateHandler(state);

        var command = new LinkGoogleAccountCommand("code", "state");
        await handler.Handle(command, CancellationToken.None);

        var link = linkRepo.AddedLinks.First();
        link.EncryptedAccessToken.Value.Should().StartWith("encrypted:");
        link.EncryptedRefreshToken.Value.Should().StartWith("encrypted:");
    }

    [Fact]
    public async Task Handle_WithValidState_ShouldRaiseDomainEvent()
    {
        var userId = UserId.New();
        var state = OAuthState.Create("state", userId, "verifier");
        var (handler, linkRepo, _, _, _) = CreateHandler(state);

        var command = new LinkGoogleAccountCommand("code", "state");
        await handler.Handle(command, CancellationToken.None);

        var link = linkRepo.AddedLinks.First();
        link.DomainEvents.Should().HaveCount(1);
        link.DomainEvents.First().Should().BeOfType<GoogleAccountLinkedEvent>();
    }

    [Fact]
    public async Task Handle_WithValidState_ShouldDeleteOAuthState()
    {
        var userId = UserId.New();
        var state = OAuthState.Create("state", userId, "verifier");
        var (handler, _, stateRepo, _, _) = CreateHandler(state);

        var command = new LinkGoogleAccountCommand("code", "state");
        await handler.Handle(command, CancellationToken.None);

        stateRepo.DeletedStates.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithInvalidState_ShouldThrow()
    {
        var (handler, _, _, _, _) = CreateHandler(existingState: null);

        var command = new LinkGoogleAccountCommand("code", "nonexistent-state");
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Invalid or expired OAuth state*");
    }

    [Fact]
    public async Task Handle_WhenAlreadyLinked_ShouldThrow()
    {
        var userId = UserId.New();
        var state = OAuthState.Create("state", userId, "verifier");

        var existingLink = FamilyHub.Api.Features.GoogleIntegration.Domain.Entities.GoogleAccountLink.Create(
            userId,
            FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects.GoogleAccountId.From("existing-sub"),
            Email.From("existing@gmail.com"),
            FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects.EncryptedToken.From("enc-access"),
            FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects.EncryptedToken.From("enc-refresh"),
            DateTime.UtcNow.AddHours(1),
            FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects.GoogleScopes.From("openid"));

        var stateRepo = new FakeOAuthStateRepository(state);
        var linkRepo = new FakeGoogleAccountLinkRepository(existingLink);
        var oauthService = new FakeGoogleOAuthService();
        var encryptionService = new FakeTokenEncryptionService();
        var handler = new LinkGoogleAccountCommandHandler(
            stateRepo, linkRepo, oauthService, encryptionService);

        var command = new LinkGoogleAccountCommand("code", "state");
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already linked*");
    }
}
