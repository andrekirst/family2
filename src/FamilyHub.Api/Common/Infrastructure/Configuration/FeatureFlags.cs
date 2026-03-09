namespace FamilyHub.Api.Common.Infrastructure.Configuration;

/// <summary>
/// Configuration-based feature flags for toggling modules on/off.
/// Bound to the "FeatureFlags" configuration section.
/// </summary>
public sealed class FeatureFlags
{
    public const string SectionName = "FeatureFlags";

    public bool EventChainEnabled { get; set; } = true;
    public bool FileManagementEnabled { get; set; } = true;
    public bool MessagingEnabled { get; set; } = true;
    public bool CalendarEnabled { get; set; } = true;
    public bool GoogleIntegrationEnabled { get; set; } = true;
    public bool DashboardEnabled { get; set; } = true;
    public bool SchoolEnabled { get; set; } = true;
}
