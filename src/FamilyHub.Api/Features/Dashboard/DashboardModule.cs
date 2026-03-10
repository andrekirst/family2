using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Common.Widgets;
using FamilyHub.Api.Features.Dashboard.Application.Search;
using FamilyHub.Api.Features.Dashboard.Domain.Repositories;
using FamilyHub.Api.Features.Dashboard.Infrastructure.Repositories;

namespace FamilyHub.Api.Features.Dashboard;

[ModuleOrder(400)]
public sealed class DashboardModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        // Widget registry (singleton — shared across all modules)
        services.AddSingleton<IWidgetRegistry, WidgetRegistry>();
        services.AddHostedService<WidgetRegistryInitializer>();

        // Dashboard repository
        services.AddScoped<IDashboardLayoutRepository, DashboardLayoutRepository>();

        // Built-in widget provider (welcome widget)
        services.AddSingleton<IWidgetProvider, DashboardWidgetProvider>();

        // Search
        services.AddScoped<ISearchProvider, DashboardSearchProvider>();
        services.AddSingleton<ICommandPaletteProvider, DashboardCommandPaletteProvider>();
    }
}
