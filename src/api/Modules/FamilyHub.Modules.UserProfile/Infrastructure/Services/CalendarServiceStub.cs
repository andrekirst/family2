using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.UserProfile.Infrastructure.Services;

/// <summary>
/// Stub implementation of ICalendarService for UserProfile module.
/// This stub logs calendar operations until the Calendar module is implemented (Phase 2).
/// When the Calendar module is created, this stub will be replaced by the real implementation.
/// </summary>
public sealed partial class CalendarServiceStub : ICalendarService
{
    private readonly ILogger<CalendarServiceStub> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarServiceStub"/> class.
    /// </summary>
    /// <param name="logger">Logger for structured logging.</param>
    public CalendarServiceStub(ILogger<CalendarServiceStub> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task CreateRecurringBirthdayEventAsync(
        FamilyId familyId,
        UserId userId,
        string displayName,
        DateOnly birthday,
        CancellationToken cancellationToken = default)
    {
        LogBirthdayEventCreation(
            familyId.Value,
            userId.Value,
            displayName,
            birthday);

        // Stub: No actual calendar event creation
        // When Calendar module is implemented, this will be replaced
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateBirthdayEventTitleAsync(
        FamilyId familyId,
        UserId userId,
        string newDisplayName,
        CancellationToken cancellationToken = default)
    {
        LogBirthdayEventTitleUpdate(
            familyId.Value,
            userId.Value,
            newDisplayName);

        // Stub: No actual calendar event update
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveBirthdayEventAsync(
        FamilyId familyId,
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        LogBirthdayEventRemoval(familyId.Value, userId.Value);

        // Stub: No actual calendar event removal
        return Task.CompletedTask;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "[STUB] Would create recurring birthday event: FamilyId={FamilyId}, UserId={UserId}, DisplayName={DisplayName}, Birthday={Birthday}")]
    private partial void LogBirthdayEventCreation(
        Guid familyId,
        Guid userId,
        string displayName,
        DateOnly birthday);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "[STUB] Would update birthday event title: FamilyId={FamilyId}, UserId={UserId}, NewDisplayName={NewDisplayName}")]
    private partial void LogBirthdayEventTitleUpdate(
        Guid familyId,
        Guid userId,
        string newDisplayName);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "[STUB] Would remove birthday event: FamilyId={FamilyId}, UserId={UserId}")]
    private partial void LogBirthdayEventRemoval(Guid familyId, Guid userId);
}
