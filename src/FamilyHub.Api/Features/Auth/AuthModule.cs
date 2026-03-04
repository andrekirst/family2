using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Auth.Application.Search;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.Infrastructure.Repositories;

namespace FamilyHub.Api.Features.Auth;

public sealed class AuthModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();

        // Search
        services.AddSingleton<ICommandPaletteProvider, ProfileCommandPaletteProvider>();
    }
}
