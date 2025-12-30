using FamilyHub.Modules.Auth.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FamilyHub.Tests.Integration.Helpers;

/// <summary>
/// Custom WebApplicationFactory for integration tests that provides TestCurrentUserService.
/// </summary>

public sealed class TestApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing ICurrentUserService registration
            services.RemoveAll<ICurrentUserService>();

            // Register TestCurrentUserService as a singleton
            services.AddSingleton<ICurrentUserService, TestCurrentUserService>();
        });

        // Add test Zitadel configuration to prevent startup errors in CI
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var testZitadelSettings = new Dictionary<string, string?>
            {
                ["Zitadel:Authority"] = "https://test.zitadel.cloud",
                ["Zitadel:ClientId"] = "test-client-id",
                ["Zitadel:ClientSecret"] = "test-client-secret",
                ["Zitadel:RedirectUri"] = "https://localhost:5001/callback",
                ["Zitadel:Scope"] = "openid profile email"
            };

            config.AddInMemoryCollection(testZitadelSettings);
        });
    }
}
