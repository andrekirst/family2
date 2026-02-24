namespace FamilyHub.Api.Features.GoogleIntegration.Models;

public class TokenRefreshOptions
{
    public const string SectionName = "GoogleIntegration:TokenRefresh";

    public int IntervalMinutes { get; set; } = 30;
    public int RefreshBeforeExpiryMinutes { get; set; } = 15;
}
