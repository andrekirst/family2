using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;

namespace FamilyHub.Api.Features.Calendar.Application.Commands.CancelCalendarEvent;

public sealed class CancelCalendarEventCommandHandler(
    ICalendarEventRepository repository)
    : ICommandHandler<CancelCalendarEventCommand, CancelCalendarEventResult>
{
    public async ValueTask<CancelCalendarEventResult> Handle(
        CancelCalendarEventCommand command,
        CancellationToken cancellationToken)
    {
        var calendarEvent = (await repository.GetByIdAsync(command.CalendarEventId, cancellationToken))!;

        calendarEvent.Cancel();

        return new CancelCalendarEventResult(true);
    }
}
