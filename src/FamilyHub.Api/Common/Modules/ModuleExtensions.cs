namespace FamilyHub.Api.Common.Modules;

public static class ModuleExtensions
{
    public static IServiceCollection RegisterModule<TModule>(
        this IServiceCollection services, IConfiguration configuration)
        where TModule : IModule, new()
    {
        var module = new TModule();
        module.Register(services, configuration);
        return services;
    }
}
