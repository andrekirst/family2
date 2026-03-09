using FluentAssertions;
using FamilyHub.Common.Domain;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Application.Commands.RefreshGoogleToken;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Services;
using FamilyHub.Api.Features.GoogleIntegration.Models;
using NSubstitute;

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
            GoogleScopes.From("openid email"), DateTimeOffset.UtcNow);
        link.ClearDomainEvents();
        return link;
    }

    [Fact]
    public async Task Handle_ShouldRefreshToken()
    {
        var userId = UserId.New();
        var existingLink = CreateTestLink(userId);

        var linkRepo = Substitute.For<IGoogleAccountLinkRepository>();
        linkRepo.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(existingLink);

        var oauthService = Substitute.For<IGoogleOAuthService>();
        oauthService.RefreshAccessTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new GoogleTokenResponse
            {
                AccessToken = "new-access-token",
                ExpiresIn = 3600,
                Scope = "openid email"
            });

        var encryptionService = Substitute.For<ITokenEncryptionService>();
        encryptionService.Encrypt(Arg.Any<string>())
            .Returns(callInfo => $"encrypted:{callInfo.ArgAt<string>(0)}");
        encryptionService.Decrypt(Arg.Any<string>())
            .Returns(callInfo => callInfo.ArgAt<string>(0).Replace("encrypted:", ""));

        var handler = new RefreshGoogleTokenCommandHandler(linkRepo, oauthService, encryptionService, TimeProvider.System);

        var command = new RefreshGoogleTokenCommand { UserId = userId };
        var result = await handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.NewExpiresAt.Should().NotBeNull();
        result.NewExpiresAt!.Value.Should().BeCloseTo(
            DateTime.UtcNow.AddSeconds(3600), TimeSpan.FromSeconds(5));

        existingLink.EncryptedAccessToken.Value.Should().Contain("new-access-token");
        existingLink.Status.Should().Be(GoogleLinkStatus.Active);
    }
}
