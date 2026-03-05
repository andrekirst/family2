using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Common.Search;

namespace FamilyHub.Api.Features.Search;

public sealed class SearchModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        // Command palette registry (singleton — shared across all modules)
        services.AddSingleton<ICommandPaletteRegistry, CommandPaletteRegistry>();
        services.AddHostedService<CommandPaletteRegistryInitializer>();
    }
}
