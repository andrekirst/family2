using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Calendar.Application.Commands;
using FamilyHub.Api.Features.Calendar.Application.Mappers;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;
using FamilyHub.Api.Features.Calendar.Domain.ValueObjects;
using FamilyHub.Api.Features.Calendar.Models;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace FamilyHub.Api.Features.Calendar.GraphQL;

[ExtendObjectType(typeof(FamilyCalendarMutation))]
public class CalendarMutations
{
    [Authorize]
    public async Task<CalendarEventDto> Create(
        CreateCalendarEventRequest input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] ICalendarEventRepository repository,
        [Service] IUserRepository userRepository,
        CancellationToken ct)
    {
        var externalUserIdString = claimsPrincipal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var externalUserId = ExternalUserId.From(externalUserIdString);
        var user = await userRepository.GetByExternalIdAsync(externalUserId, ct)
            ?? throw new UnauthorizedAccessException("User not found");

        if (user.FamilyId is null)
        {
            throw new InvalidOperationException("User must belong to a family to create events");
        }

        var title = EventTitle.From(input.Title.Trim());
        var attendeeIds = input.AttendeeIds.Select(id => UserId.From(id)).ToList();

        var command = new CreateCalendarEventCommand(
            user.FamilyId.Value,
            user.Id,
            title,
            input.Description?.Trim(),
            input.Location?.Trim(),
            input.StartTime,
            input.EndTime,
            input.IsAllDay,
            attendeeIds);

        var result = await commandBus.SendAsync<CreateCalendarEventResult>(command, ct);

        var created = await repository.GetByIdWithAttendeesAsync(result.CalendarEventId, ct)
            ?? throw new InvalidOperationException("Event creation failed");

        return CalendarEventMapper.ToDto(created);
    }

    [Authorize]
    public async Task<CalendarEventDto> Update(
        Guid id,
        UpdateCalendarEventRequest input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] ICalendarEventRepository repository,
        [Service] IUserRepository userRepository,
        CancellationToken ct)
    {
        var externalUserIdString = claimsPrincipal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var externalUserId = ExternalUserId.From(externalUserIdString);
        _ = await userRepository.GetByExternalIdAsync(externalUserId, ct)
            ?? throw new UnauthorizedAccessException("User not found");

        var calendarEventId = CalendarEventId.From(id);
        var title = EventTitle.From(input.Title.Trim());
        var attendeeIds = input.AttendeeIds.Select(uid => UserId.From(uid)).ToList();

        var command = new UpdateCalendarEventCommand(
            calendarEventId,
            title,
            input.Description?.Trim(),
            input.Location?.Trim(),
            input.StartTime,
            input.EndTime,
            input.IsAllDay,
            attendeeIds);

        var result = await commandBus.SendAsync<UpdateCalendarEventResult>(command, ct);

        var updated = await repository.GetByIdWithAttendeesAsync(result.CalendarEventId, ct)
            ?? throw new InvalidOperationException("Event update failed");

        return CalendarEventMapper.ToDto(updated);
    }

    [Authorize]
    public async Task<bool> Cancel(
        Guid id,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICommandBus commandBus,
        [Service] IUserRepository userRepository,
        CancellationToken ct)
    {
        var externalUserIdString = claimsPrincipal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var externalUserId = ExternalUserId.From(externalUserIdString);
        _ = await userRepository.GetByExternalIdAsync(externalUserId, ct)
            ?? throw new UnauthorizedAccessException("User not found");

        var calendarEventId = CalendarEventId.From(id);
        var command = new CancelCalendarEventCommand(calendarEventId);

        var result = await commandBus.SendAsync<CancelCalendarEventResult>(command, ct);
        return result.Success;
    }
}
