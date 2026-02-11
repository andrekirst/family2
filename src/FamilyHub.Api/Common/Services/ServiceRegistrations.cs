namespace FamilyHub.Api.Common.Services;

public static class ServiceRegistrations
{
    public static IServiceCollection AddFamilyServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
