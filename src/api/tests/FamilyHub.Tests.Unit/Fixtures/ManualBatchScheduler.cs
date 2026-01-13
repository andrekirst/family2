using GreenDonut;

namespace FamilyHub.Tests.Unit.Fixtures;

/// <summary>
/// A batch scheduler for unit tests that provides explicit control over batch dispatch timing.
/// Unlike AutoBatchScheduler which uses timing-based dispatch (non-deterministic in CI),
/// ManualBatchScheduler requires explicit DispatchAsync() calls for deterministic testing.
/// </summary>
/// <remarks>
/// Use this scheduler when testing DataLoader batching behavior to ensure all queued
/// requests are processed together in a single batch, regardless of system timing.
/// </remarks>
public sealed class ManualBatchScheduler : IBatchScheduler
{
    private readonly List<Func<ValueTask>> _dispatchActions = [];

    /// <summary>
    /// Schedules a dispatch action to be executed when DispatchAsync is called.
    /// This stores the action without executing it immediately.
    /// </summary>
    /// <param name="dispatch">The batch dispatch action from the DataLoader.</param>
    public void Schedule(Func<ValueTask> dispatch)
    {
        _dispatchActions.Add(dispatch);
    }

    /// <summary>
    /// Executes all pending dispatch actions in order.
    /// Call this after queuing all LoadAsync requests to trigger batch processing.
    /// </summary>
    /// <returns>A task that completes when all batches have been dispatched.</returns>
    public async ValueTask DispatchAsync()
    {
        var actions = _dispatchActions.ToList();
        _dispatchActions.Clear();

        foreach (var action in actions)
        {
            await action();
        }
    }

    /// <summary>
    /// Gets the number of pending dispatch actions.
    /// Useful for verifying that batches were scheduled.
    /// </summary>
    public int PendingCount => _dispatchActions.Count;
}
