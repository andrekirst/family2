using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.GoogleIntegration.Domain.Entities;

public sealed class OAuthState
{
#pragma warning disable CS8618
    private OAuthState() { }
#pragma warning restore CS8618

    public string State { get; private set; }
    public UserId UserId { get; private set; }
    public string CodeVerifier { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    public static OAuthState Create(string state, UserId userId, string codeVerifier)
    {
        return new OAuthState
        {
            State = state,
            UserId = userId,
            CodeVerifier = codeVerifier,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
    }

    public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;
}
