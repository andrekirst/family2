using FamilyHub.Api.Common.Email;
using FamilyHub.Api.Features.Family.Infrastructure.Avatar;
using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Common.Widgets;
using FamilyHub.Api.Features.Family.Application.Search;
using FamilyHub.Api.Features.Family.Application.Services;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Infrastructure.Repositories;

namespace FamilyHub.Api.Features.Family;

[ModuleOrder(200)]
public sealed class FamilyModule : IModule, IEndpointModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IFamilyRepository, FamilyRepository>();
        services.AddScoped<IFamilyMemberRepository, FamilyMemberRepository>();
        services.AddScoped<IFamilyInvitationRepository, FamilyInvitationRepository>();
        services.AddScoped<FamilyAuthorizationService>();

        // Avatar infrastructure (Family module owns avatar management)
        services.AddScoped<IAvatarRepository, AvatarRepository>();
        services.AddScoped<IFileStorageService, PostgresFileStorageService>();
        services.AddScoped<IAvatarProcessingService, AvatarProcessingService>();

        services.Configure<EmailConfiguration>(configuration.GetSection("Email"));
        services.AddScoped<IEmailService, SmtpEmailService>();

        // Widget provider for dashboard
        services.AddSingleton<IWidgetProvider, FamilyWidgetProvider>();

        // Search & command palette providers
        services.AddScoped<ISearchProvider, FamilySearchProvider>();
        services.AddSingleton<ICommandPaletteProvider, FamilyCommandPaletteProvider>();
    }

    public void MapEndpoints(WebApplication app)
    {
        app.MapGet("/api/avatars/{avatarId:guid}/{size}",
            AvatarEndpoints.GetAvatar)
            .RequireAuthorization();
    }
}
