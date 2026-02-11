namespace FamilyHub.Api.Common.Modules;

public interface IModule
{
    void Register(IServiceCollection services, IConfiguration configuration);
}
