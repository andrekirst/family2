using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Calendar.Application.Commands.CancelCalendarEvent;

public sealed record CancelCalendarEventCommand(
    CalendarEventId CalendarEventId,
    FamilyId FamilyId
) : ICommand<CancelCalendarEventResult>, IFamilyScoped;
