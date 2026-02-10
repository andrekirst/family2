using FamilyHub.Api.Common.Domain;
using FamilyHub.Api.Features.Calendar.Application.Commands;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;

namespace FamilyHub.Api.Features.Calendar.Application.Handlers;

public static class CancelCalendarEventCommandHandler
{
    public static async Task<CancelCalendarEventResult> Handle(
        CancelCalendarEventCommand command,
        ICalendarEventRepository repository,
        CancellationToken ct)
    {
        var calendarEvent = await repository.GetByIdAsync(command.CalendarEventId, ct)
            ?? throw new DomainException("Calendar event not found");

        calendarEvent.Cancel();
        await repository.SaveChangesAsync(ct);

        return new CancelCalendarEventResult(true);
    }
}
