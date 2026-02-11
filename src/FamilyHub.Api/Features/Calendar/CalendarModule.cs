using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;
using FamilyHub.Api.Features.Calendar.Infrastructure.Repositories;
using FamilyHub.Api.Features.Calendar.Infrastructure.Services;
using FamilyHub.Api.Features.Calendar.Models;

namespace FamilyHub.Api.Features.Calendar;

public sealed class CalendarModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICalendarEventRepository, CalendarEventRepository>();

        services.Configure<CalendarCleanupOptions>(
            configuration.GetSection(CalendarCleanupOptions.SectionName));
        services.AddHostedService<CancelledEventCleanupService>();
    }
}
