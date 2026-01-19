using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Infrastructure.Authorization;
using FamilyHub.Modules.Auth.Infrastructure.Configuration;
using FamilyHub.Modules.Auth.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FamilyHub.Modules.Auth;

/// <summary>
/// Provides test mode service registration for E2E testing.
/// </summary>
/// <remarks>
/// <para>
/// Test mode enables header-based authentication for Playwright E2E tests.
/// When enabled, <see cref="HeaderBasedCurrentUserService"/> replaces the standard
/// <see cref="CurrentUserService"/>, allowing tests to authenticate by sending
/// X-Test-User-Id and X-Test-User-Email headers.
/// </para>
/// <para>
/// SECURITY: Test mode is blocked in Production environment to prevent
/// accidentally exposing a backdoor authentication mechanism.
/// </para>
/// </remarks>
public static class TestModeServiceRegistration
{
    /// <summary>
    /// Configures test mode services if enabled.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environmentName">The current environment name (e.g., "Development", "Test", "Production").</param>
    /// <returns>True if test mode was enabled, false otherwise.</returns>
    /// <exception cref="InvalidOperationException">Thrown if test mode is enabled in Production environment.</exception>
    /// <remarks>
    /// <para>
    /// Test mode can be enabled in two ways:
    /// <list type="bullet">
    /// <item><description>Environment variable: FAMILYHUB_TEST_MODE=true</description></item>
    /// <item><description>Configuration: TestMode:Enabled=true in appsettings.json</description></item>
    /// </list>
    /// The environment variable takes precedence over configuration.
    /// </para>
    /// <para>
    /// When test mode is enabled, this method:
    /// <list type="number">
    /// <item><description>Validates that the environment is not Production</description></item>
    /// <item><description>Replaces ICurrentUserService with HeaderBasedCurrentUserService</description></item>
    /// <item><description>Replaces all IAuthorizationHandler instances with TestAuthorizationHandler</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static bool TryAddTestMode(
        this IServiceCollection services,
        IConfiguration configuration,
        string environmentName)
    {
        // Check environment variable first, then appsettings
        var envVar = Environment.GetEnvironmentVariable(TestModeSettings.EnvironmentVariable);
        var testModeEnabled = bool.TryParse(envVar, out var envEnabled) && envEnabled;

        if (!testModeEnabled)
        {
            var settings = configuration.GetSection(TestModeSettings.SectionName).Get<TestModeSettings>();
            testModeEnabled = settings?.Enabled ?? false;
        }

        if (!testModeEnabled)
        {
            return false;
        }

        // CRITICAL: Validate not running in production
        var testModeSettings = new TestModeSettings { Enabled = true };
        testModeSettings.ValidateNotProduction(environmentName);

        // Use Console.WriteLine for startup logging since this runs before DI is configured
        // and we don't want to add a Serilog package dependency to this module
        Console.WriteLine(
            $"⚠️  TEST MODE ENABLED - Using header-based authentication. " +
            $"JWT validation is bypassed. Environment: {environmentName}");

        // Replace ICurrentUserService with header-based implementation
        // RemoveAll ensures the original CurrentUserService is removed
        services.RemoveAll<ICurrentUserService>();
        services.AddScoped<ICurrentUserService, HeaderBasedCurrentUserService>();

        // Replace authorization handlers with test handler
        // This allows all authorization policies to pass
        services.RemoveAll<IAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, TestAuthorizationHandler>();

        Console.WriteLine(
            $"ℹ️  Test mode authentication configured. " +
            $"Use headers '{HeaderBasedCurrentUserService.TestUserIdHeader}' and " +
            $"'{HeaderBasedCurrentUserService.TestUserEmailHeader}' for authentication.");

        return true;
    }
}
