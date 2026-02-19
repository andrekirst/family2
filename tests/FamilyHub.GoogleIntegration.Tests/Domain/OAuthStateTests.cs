using FluentAssertions;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;

namespace FamilyHub.GoogleIntegration.Tests.Domain;

public class OAuthStateTests
{
    [Fact]
    public void Create_ShouldSetPropertiesCorrectly()
    {
        var userId = UserId.New();
        var state = OAuthState.Create("random-state", userId, "code-verifier");

        state.State.Should().Be("random-state");
        state.UserId.Should().Be(userId);
        state.CodeVerifier.Should().Be("code-verifier");
        state.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(10), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void IsExpired_ShouldReturnFalseWhenFresh()
    {
        var state = OAuthState.Create("state", UserId.New(), "verifier");
        state.IsExpired().Should().BeFalse();
    }
}
