using FamilyHub.Modules.Family.Domain.Specifications;
using FamilyHub.SharedKernel.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace FamilyHub.Modules.Auth.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job that permanently deletes expired invitations older than 30 days.
/// Runs daily at 3 AM UTC.
/// </summary>
[DisallowConcurrentExecution]
public partial class ExpiredInvitationCleanupJob(
    IServiceProvider serviceProvider,
    ILogger<ExpiredInvitationCleanupJob> logger)
    : IJob
{
    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        LogJobStarting();

        using var scope = serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFamilyMemberInvitationRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            // Get expired invitations older than 30 days using Specification pattern
            var expirationThreshold = DateTime.UtcNow.AddDays(-30);
            var expiredInvitations = await repository.FindAllAsync(
                new ExpiredInvitationForCleanupSpecification(expirationThreshold),
                context.CancellationToken);

            if (expiredInvitations.Count == 0)
            {
                LogNoExpiredInvitations();
                return;
            }

            LogFoundCountExpiredInvitationsToCleanUpExpiredDaysAgo(logger, expiredInvitations.Count);

            // Delete invitations permanently
            repository.RemoveInvitations(expiredInvitations);
            await unitOfWork.SaveChangesAsync(context.CancellationToken);

            LogSuccessfullyDeletedCountExpiredInvitations(logger, expiredInvitations.Count);
        }
        catch (Exception ex)
        {
            LogJobError(ex);
            throw;
        }
    }

    [LoggerMessage(LogLevel.Information, "Found {count} expired invitations to clean up (expired >30 days ago)")]
    static partial void LogFoundCountExpiredInvitationsToCleanUpExpiredDaysAgo(ILogger<ExpiredInvitationCleanupJob> logger, int count);

    [LoggerMessage(LogLevel.Information, "Successfully deleted {count} expired invitations")]
    static partial void LogSuccessfullyDeletedCountExpiredInvitations(ILogger<ExpiredInvitationCleanupJob> logger, int count);

    [LoggerMessage(LogLevel.Information, "ExpiredInvitationCleanupJob starting execution")]
    partial void LogJobStarting();

    [LoggerMessage(LogLevel.Information, "No expired invitations to clean up")]
    partial void LogNoExpiredInvitations();

    [LoggerMessage(LogLevel.Error, "Error executing ExpiredInvitationCleanupJob")]
    partial void LogJobError(Exception exception);
}
