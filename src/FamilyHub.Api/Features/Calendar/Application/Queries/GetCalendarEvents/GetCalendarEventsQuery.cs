using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Calendar.Models;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Calendar.Application.Queries.GetCalendarEvents;

public sealed record GetCalendarEventsQuery(
    FamilyId FamilyId,
    DateTime StartDate,
    DateTime EndDate
) : IReadOnlyQuery<List<CalendarEventDto>>, IFamilyScoped;
