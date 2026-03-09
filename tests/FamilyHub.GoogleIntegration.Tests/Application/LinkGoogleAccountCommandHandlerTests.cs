using FluentAssertions;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Application.Commands.LinkGoogleAccount;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Events;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Services;
using FamilyHub.Api.Features.GoogleIntegration.Models;
using NSubstitute;

namespace FamilyHub.GoogleIntegration.Tests.Application;

public class LinkGoogleAccountCommandHandlerTests
{
    private static (LinkGoogleAccountCommandHandler Handler,
        IGoogleAccountLinkRepository LinkRepo,
        IOAuthStateRepository StateRepo,
        IGoogleOAuthService OAuthService,
        ITokenEncryptionService EncryptionService)
        CreateHandler(OAuthState? existingState = null)
    {
        var stateRepo = Substitute.For<IOAuthStateRepository>();
        if (existingState is not null)
        {
            stateRepo.GetByStateAsync(existingState.State, Arg.Any<CancellationToken>())
                .Returns(existingState);
        }

        var linkRepo = Substitute.For<IGoogleAccountLinkRepository>();

        var oauthService = Substitute.For<IGoogleOAuthService>();
        oauthService.ExchangeCodeForTokensAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new GoogleTokenResponse
            {
                AccessToken = "fake-access-token",
                RefreshToken = "fake-refresh-token",
                ExpiresIn = 3600,
                TokenType = "Bearer",
                Scope = "openid email profile https://www.googleapis.com/auth/calendar.readonly https://www.googleapis.com/auth/calendar.events"
            });
        oauthService.GetUserInfoAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new GoogleUserInfo
            {
                Sub = "google-user-123",
                Email = "test@gmail.com"
            });

        var encryptionService = Substitute.For<ITokenEncryptionService>();
        encryptionService.Encrypt(Arg.Any<string>())
            .Returns(callInfo => $"encrypted:{callInfo.ArgAt<string>(0)}");
        encryptionService.Decrypt(Arg.Any<string>())
            .Returns(callInfo => callInfo.ArgAt<string>(0).Replace("encrypted:", ""));

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

        GoogleAccountLink? capturedLink = null;
        await linkRepo.AddAsync(Arg.Do<GoogleAccountLink>(l => capturedLink = l), Arg.Any<CancellationToken>());

        var command = new LinkGoogleAccountCommand("auth-code", "valid-state");
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        await linkRepo.Received(1).AddAsync(Arg.Any<GoogleAccountLink>(), Arg.Any<CancellationToken>());

        capturedLink.Should().NotBeNull();
        capturedLink!.UserId.Should().Be(userId);
        capturedLink.GoogleAccountId.Value.Should().Be("google-user-123");
        capturedLink.GoogleEmail.Value.Should().Be("test@gmail.com");
    }

    [Fact]
    public async Task Handle_WithValidState_ShouldEncryptTokens()
    {
        var userId = UserId.New();
        var state = OAuthState.Create("state", userId, "verifier");
        var (handler, linkRepo, _, _, _) = CreateHandler(state);

        GoogleAccountLink? capturedLink = null;
        await linkRepo.AddAsync(Arg.Do<GoogleAccountLink>(l => capturedLink = l), Arg.Any<CancellationToken>());

        var command = new LinkGoogleAccountCommand("code", "state");
        await handler.Handle(command, CancellationToken.None);

        capturedLink.Should().NotBeNull();
        capturedLink!.EncryptedAccessToken.Value.Should().StartWith("encrypted:");
        capturedLink.EncryptedRefreshToken.Value.Should().StartWith("encrypted:");
    }

    [Fact]
    public async Task Handle_WithValidState_ShouldRaiseDomainEvent()
    {
        var userId = UserId.New();
        var state = OAuthState.Create("state", userId, "verifier");
        var (handler, linkRepo, _, _, _) = CreateHandler(state);

        GoogleAccountLink? capturedLink = null;
        await linkRepo.AddAsync(Arg.Do<GoogleAccountLink>(l => capturedLink = l), Arg.Any<CancellationToken>());

        var command = new LinkGoogleAccountCommand("code", "state");
        await handler.Handle(command, CancellationToken.None);

        capturedLink.Should().NotBeNull();
        capturedLink!.DomainEvents.Should().HaveCount(1);
        capturedLink.DomainEvents.First().Should().BeOfType<GoogleAccountLinkedEvent>();
    }

    [Fact]
    public async Task Handle_WithValidState_ShouldDeleteOAuthState()
    {
        var userId = UserId.New();
        var state = OAuthState.Create("state", userId, "verifier");
        var (handler, _, stateRepo, _, _) = CreateHandler(state);

        var command = new LinkGoogleAccountCommand("code", "state");
        await handler.Handle(command, CancellationToken.None);

        await stateRepo.Received(1).DeleteAsync(
            Arg.Is<OAuthState>(s => s.State == "state"),
            Arg.Any<CancellationToken>());
    }
}
