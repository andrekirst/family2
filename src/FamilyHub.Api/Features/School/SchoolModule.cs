using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.School.Application.Search;
using FamilyHub.Api.Features.School.Domain.Repositories;
using FamilyHub.Api.Features.School.Infrastructure.Repositories;

namespace FamilyHub.Api.Features.School;

[ModuleOrder(1000)]
public sealed class SchoolModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<ISchoolYearRepository, SchoolYearRepository>();
        services.AddScoped<IClassAssignmentRepository, ClassAssignmentRepository>();
        services.AddSingleton<ICommandPaletteProvider, SchoolCommandPaletteProvider>();
        services.AddScoped<ISearchProvider, SchoolSearchProvider>();
    }
}
