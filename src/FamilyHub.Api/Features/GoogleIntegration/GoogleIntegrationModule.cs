using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Features.GoogleIntegration.Domain.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Repositories;
using FamilyHub.Api.Features.GoogleIntegration.Infrastructure.Services;
using FamilyHub.Api.Features.GoogleIntegration.Models;

namespace FamilyHub.Api.Features.GoogleIntegration;

public sealed class GoogleIntegrationModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.Configure<GoogleIntegrationOptions>(
            configuration.GetSection(GoogleIntegrationOptions.SectionName));
        services.Configure<GoogleOAuthOptions>(
            configuration.GetSection(GoogleOAuthOptions.SectionName));
        services.Configure<TokenRefreshOptions>(
            configuration.GetSection(TokenRefreshOptions.SectionName));

        // Repositories
        services.AddScoped<IGoogleAccountLinkRepository, GoogleAccountLinkRepository>();
        services.AddScoped<IOAuthStateRepository, OAuthStateRepository>();

        // Services
        services.AddSingleton<ITokenEncryptionService, AesGcmTokenEncryptionService>();
        services.AddHttpClient<IGoogleOAuthService, GoogleOAuthService>();

        // Background services
        services.AddHostedService<TokenRefreshBackgroundService>();
    }
}
