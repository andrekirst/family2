using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.Calendar.Application.Commands;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;

namespace FamilyHub.Api.Features.Calendar.Application.Handlers;

public sealed class CancelCalendarEventCommandHandler(
    ICalendarEventRepository repository)
    : ICommandHandler<CancelCalendarEventCommand, CancelCalendarEventResult>
{
    public async ValueTask<CancelCalendarEventResult> Handle(
        CancelCalendarEventCommand command,
        CancellationToken cancellationToken)
    {
        var calendarEvent = await repository.GetByIdAsync(command.CalendarEventId, cancellationToken)
            ?? throw new DomainException("Calendar event not found", DomainErrorCodes.CalendarEventNotFound);

        calendarEvent.Cancel();
        await repository.SaveChangesAsync(cancellationToken);

        return new CancelCalendarEventResult(true);
    }
}
