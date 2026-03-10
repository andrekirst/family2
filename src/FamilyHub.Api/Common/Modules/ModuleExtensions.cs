namespace FamilyHub.Api.Common.Modules;

public static class ModuleExtensions
{
    private static readonly List<IModule> RegisteredModules = [];

    public static IServiceCollection RegisterModule<TModule>(
        this IServiceCollection services, IConfiguration configuration)
        where TModule : IModule, new()
    {
        var module = new TModule();
        module.Register(services, configuration);
        RegisteredModules.Add(module);
        return services;
    }

    /// <summary>
    /// Maps endpoints for all registered modules that implement IEndpointModule.
    /// Call this after all modules are registered and the app is built.
    /// </summary>
    public static WebApplication MapModuleEndpoints(this WebApplication app)
    {
        foreach (var module in RegisteredModules.OfType<IEndpointModule>())
        {
            module.MapEndpoints(app);
        }
        return app;
    }
}
