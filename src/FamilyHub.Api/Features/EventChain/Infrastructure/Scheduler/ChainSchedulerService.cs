using FamilyHub.Api.Common.Database;
using FamilyHub.EventChain.Infrastructure.Orchestrator;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Features.EventChain.Infrastructure.Scheduler;

public sealed class ChainSchedulerService(
    IServiceScopeFactory scopeFactory,
    ILogger<ChainSchedulerService> logger) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan StaleJobTimeout = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Chain scheduler started. Polling interval: {Interval}s", PollingInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessReadyJobsAsync(stoppingToken);
                await ProcessStaleJobsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Scheduler error during poll cycle");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task ProcessReadyJobsAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var orchestrator = scope.ServiceProvider.GetRequiredService<IChainOrchestrator>();

        // Use raw SQL for SELECT FOR UPDATE SKIP LOCKED (not expressible in LINQ)
        var readyJobs = await context.ChainScheduledJobs
            .FromSqlRaw("""
                SELECT * FROM event_chain.chain_scheduled_jobs
                WHERE scheduled_at <= now()
                  AND picked_up_at IS NULL
                  AND completed_at IS NULL
                  AND failed_at IS NULL
                ORDER BY scheduled_at ASC
                LIMIT 10
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(ct);

        foreach (var job in readyJobs)
        {
            try
            {
                job.PickUp();
                await context.SaveChangesAsync(ct);

                await orchestrator.ResumeStepAsync(job.StepExecutionId, ct);

                job.MarkCompleted();
                await context.SaveChangesAsync(ct);

                logger.LogInformation(
                    "Scheduled job {JobId} completed for step {StepId}",
                    job.Id, job.StepExecutionId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Scheduled job {JobId} failed for step {StepId}",
                    job.Id, job.StepExecutionId);

                job.MarkFailed(ex.Message);
                await context.SaveChangesAsync(ct);
            }
        }
    }

    private async Task ProcessStaleJobsAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var staleJobs = await context.ChainScheduledJobs
            .FromSqlRaw("""
                SELECT * FROM event_chain.chain_scheduled_jobs
                WHERE picked_up_at IS NOT NULL
                  AND completed_at IS NULL
                  AND failed_at IS NULL
                  AND picked_up_at < now() - INTERVAL '5 minutes'
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(ct);

        foreach (var job in staleJobs)
        {
            logger.LogWarning(
                "Resetting stale job {JobId} (picked up at {PickedUpAt})",
                job.Id, job.PickedUpAt);

            job.Reset();
            await context.SaveChangesAsync(ct);
        }
    }
}
