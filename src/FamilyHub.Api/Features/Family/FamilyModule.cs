using FamilyHub.Api.Common.Email;
using FamilyHub.Api.Common.Infrastructure.Avatar;
using FamilyHub.Api.Common.Modules;
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

        // Avatar infrastructure (cross-cutting, registered here as Family module owns avatar management)
        services.AddScoped<IAvatarRepository, AvatarRepository>();
        services.AddScoped<IFileStorageService, PostgresFileStorageService>();
        services.AddScoped<IAvatarProcessingService, AvatarProcessingService>();

        services.Configure<EmailConfiguration>(configuration.GetSection("Email"));
        services.AddScoped<IEmailService, SmtpEmailService>();
    }
}
