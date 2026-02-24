using FluentAssertions;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Application.Commands.RefreshGoogleToken;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Models;
using FamilyHub.TestCommon.Fakes;

namespace FamilyHub.GoogleIntegration.Tests.Application;

public class RefreshGoogleTokenCommandHandlerTests
{
    private static GoogleAccountLink CreateTestLink(UserId userId)
    {
        var link = GoogleAccountLink.Create(
            userId,
            GoogleAccountId.From("google-sub"),
            Email.From("test@gmail.com"),
            EncryptedToken.From("encrypted:old-access"),
            EncryptedToken.From("encrypted:old-refresh"),
            DateTime.UtcNow.AddMinutes(-5), // expired
            GoogleScopes.From("openid email"));
        link.ClearDomainEvents();
        return link;
    }

    [Fact]
    public async Task Handle_ShouldRefreshToken()
    {
        var userId = UserId.New();
        var existingLink = CreateTestLink(userId);
        var linkRepo = new FakeGoogleAccountLinkRepository(existingLink);
        var oauthService = new FakeGoogleOAuthService
        {
            TokenResponse = new GoogleTokenResponse
            {
                AccessToken = "new-access-token",
                ExpiresIn = 3600,
                Scope = "openid email"
            }
        };
        var encryptionService = new FakeTokenEncryptionService();
        var handler = new RefreshGoogleTokenCommandHandler(linkRepo, oauthService, encryptionService);

        var command = new RefreshGoogleTokenCommand(userId);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.NewExpiresAt.Should().NotBeNull();
        result.NewExpiresAt!.Value.Should().BeCloseTo(
            DateTime.UtcNow.AddSeconds(3600), TimeSpan.FromSeconds(5));

        existingLink.EncryptedAccessToken.Value.Should().Contain("new-access-token");
        existingLink.Status.Should().Be(GoogleLinkStatus.Active);
    }

    [Fact]
    public async Task Handle_WhenNoLinkedAccount_ShouldThrow()
    {
        var linkRepo = new FakeGoogleAccountLinkRepository();
        var oauthService = new FakeGoogleOAuthService();
        var encryptionService = new FakeTokenEncryptionService();
        var handler = new RefreshGoogleTokenCommandHandler(linkRepo, oauthService, encryptionService);

        var command = new RefreshGoogleTokenCommand(UserId.New());
        var act = () => handler.Handle(command, CancellationToken.None).AsTask();

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*No Google account linked*");
    }
}
