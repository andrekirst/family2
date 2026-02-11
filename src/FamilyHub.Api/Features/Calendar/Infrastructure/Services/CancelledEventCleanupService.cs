using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.Calendar.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FamilyHub.Api.Features.Calendar.Infrastructure.Services;

public sealed class CancelledEventCleanupService(
    IServiceScopeFactory scopeFactory,
    IOptions<CalendarCleanupOptions> options,
    ILogger<CancelledEventCleanupService> logger) : BackgroundService
{
    private readonly CalendarCleanupOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Cancelled event cleanup service started (retention: {RetentionDays} days, interval: {IntervalHours} hours)",
            _options.RetentionDays, _options.IntervalHours);

        using var timer = new PeriodicTimer(TimeSpan.FromHours(_options.IntervalHours));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await CleanupCancelledEventsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error during cancelled event cleanup");
            }
        }
    }

    private async Task CleanupCancelledEventsAsync(CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddDays(-_options.RetentionDays);

        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var deleted = await dbContext.CalendarEvents
            .Where(e => e.IsCancelled && e.UpdatedAt < cutoff)
            .ExecuteDeleteAsync(ct);

        if (deleted > 0)
        {
            logger.LogInformation("Cleaned up {Count} cancelled calendar event(s) older than {Cutoff:u}", deleted, cutoff);
        }
        else
        {
            logger.LogDebug("No cancelled calendar events to clean up");
        }
    }
}
