using FamilyHub.Api.Common.Email;
using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Common.Widgets;
using FamilyHub.Api.Features.Family.Application.Services;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Infrastructure.Repositories;

namespace FamilyHub.Api.Features.Family;

public sealed class FamilyModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IFamilyRepository, FamilyRepository>();
        services.AddScoped<IFamilyMemberRepository, FamilyMemberRepository>();
        services.AddScoped<IFamilyInvitationRepository, FamilyInvitationRepository>();
        services.AddScoped<FamilyAuthorizationService>();

        services.Configure<EmailConfiguration>(configuration.GetSection("Email"));
        services.AddScoped<IEmailService, SmtpEmailService>();

        // Widget provider for dashboard
        services.AddSingleton<IWidgetProvider, FamilyWidgetProvider>();
    }
}
