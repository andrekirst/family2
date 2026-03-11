using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Features.BaseData.Domain.Repositories;
using FamilyHub.Api.Features.BaseData.Infrastructure;
using FamilyHub.Api.Features.BaseData.Infrastructure.Repositories;

namespace FamilyHub.Api.Features.BaseData;

[ModuleOrder(1200)]
public sealed class BaseDataModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IFederalStateRepository, FederalStateRepository>();
        services.AddHostedService<BaseDataSeeder>();
    }
}
