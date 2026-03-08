using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Calendar.Application.Commands.CancelCalendarEvent;
using FamilyHub.Api.Features.Calendar.Application.Commands.CreateCalendarEvent;
using FamilyHub.Api.Features.Calendar.Application.Commands.UpdateCalendarEvent;
using FamilyHub.Api.Features.Calendar.Application.Mappers;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;
using FamilyHub.Api.Features.Calendar.Models;
using FamilyHub.Common.Domain.ValueObjects;
using HotChocolate.Authorization;

namespace FamilyHub.Api.Features.Calendar.GraphQL;

[ExtendObjectType(typeof(FamilyCalendarMutation))]
public class CalendarMutations
{
    [Authorize]
    public async Task<CalendarEventDto> Create(
        CreateCalendarEventRequest input,
        [Service] ICommandBus commandBus,
        [Service] ICalendarEventRepository repository,
        CancellationToken ct)
    {
        var title = EventTitle.From(input.Title.Trim());
        var attendeeIds = input.AttendeeIds.Select(UserId.From).ToList();

        var command = new CreateCalendarEventCommand(
            title,
            input.Description?.Trim(),
            input.Location?.Trim(),
            input.StartTime,
            input.EndTime,
            input.IsAllDay,
            attendeeIds);

        var result = await commandBus.SendAsync(command, ct);

        var created = await repository.GetByIdWithAttendeesAsync(result.CalendarEventId, ct)
            ?? throw new InvalidOperationException("Event creation failed");

        return CalendarEventMapper.ToDto(created);
    }

    [Authorize]
    public async Task<CalendarEventDto> Update(
        Guid id,
        UpdateCalendarEventRequest input,
        [Service] ICommandBus commandBus,
        [Service] ICalendarEventRepository repository,
        CancellationToken ct)
    {
        var calendarEventId = CalendarEventId.From(id);
        var title = EventTitle.From(input.Title.Trim());
        var attendeeIds = input.AttendeeIds.Select(UserId.From).ToList();

        var command = new UpdateCalendarEventCommand(
            calendarEventId,
            title,
            input.Description?.Trim(),
            input.Location?.Trim(),
            input.StartTime,
            input.EndTime,
            input.IsAllDay,
            attendeeIds);

        var result = await commandBus.SendAsync(command, ct);

        var updated = await repository.GetByIdWithAttendeesAsync(result.CalendarEventId, ct)
            ?? throw new InvalidOperationException("Event update failed");

        return CalendarEventMapper.ToDto(updated);
    }

    [Authorize]
    public async Task<bool> Cancel(
        Guid id,
        [Service] ICommandBus commandBus,
        CancellationToken ct)
    {
        var calendarEventId = CalendarEventId.From(id);
        var command = new CancelCalendarEventCommand(calendarEventId);

        var result = await commandBus.SendAsync(command, ct);
        return result.Success;
    }
}
