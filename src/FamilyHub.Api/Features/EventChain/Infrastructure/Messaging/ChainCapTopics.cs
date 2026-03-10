namespace FamilyHub.Api.Features.EventChain.Infrastructure.Messaging;

/// <summary>
/// CAP message topic constants for Event Chain messaging.
/// Topics follow the pattern: chain.{action} for RabbitMQ routing.
/// </summary>
public static class ChainCapTopics
{
    /// <summary>
    /// Published when a chain execution is created and ready to run.
    /// Subscriber picks it up and executes the chain's steps sequentially.
    /// </summary>
    public const string ExecutionRun = "chain.execution.run";

    /// <summary>
    /// Published when a scheduled step is due for execution.
    /// Replaces the DB polling scheduler with event-driven processing.
    /// </summary>
    public const string ScheduledStepReady = "chain.step.scheduled-ready";
}
