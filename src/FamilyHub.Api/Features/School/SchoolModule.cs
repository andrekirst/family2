using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.School.Application.Search;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.Api.Features.School.Infrastructure.Repositories;

namespace FamilyHub.Api.Features.School;

public sealed class SchoolModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddSingleton<ICommandPaletteProvider, SchoolCommandPaletteProvider>();
        services.AddScoped<ISearchProvider, SchoolSearchProvider>();
    }
}
