using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace FamilyHub.SharedKernel.Infrastructure.Diagnostics;

/// <summary>
/// Subscribes to specification diagnostic events and logs them.
/// </summary>
/// <param name="logger">The logger for diagnostic output.</param>
public sealed partial class SpecificationDiagnosticSubscriber(ILogger<SpecificationDiagnosticSubscriber> logger)
    : IObserver<DiagnosticListener>, IDisposable
{
    private readonly List<IDisposable> _subscriptions = [];

    /// <summary>
    /// Activates the subscriber by subscribing to all diagnostic listeners.
    /// </summary>
    /// <returns>An IDisposable to unsubscribe.</returns>
    public IDisposable Activate()
    {
        var subscription = DiagnosticListener.AllListeners.Subscribe(this);
        _subscriptions.Add(subscription);
        return subscription;
    }

    /// <inheritdoc/>
    public void OnNext(DiagnosticListener value)
    {
        if (value.Name == SpecificationDiagnosticEvents.ListenerName)
        {
            var subscription = value.Subscribe(new SpecificationEventObserver(logger));
            _subscriptions.Add(subscription);
        }
    }

    /// <inheritdoc/>
    public void OnCompleted() { }

    /// <inheritdoc/>
    public void OnError(Exception error)
    {
        LogDiagnosticError(error);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var subscription in _subscriptions)
        {
            subscription.Dispose();
        }
        _subscriptions.Clear();
    }

    /// <summary>
    /// Observer that handles individual specification events.
    /// </summary>
    /// <param name="logger">The logger for diagnostic output.</param>
    private sealed class SpecificationEventObserver(ILogger logger) : IObserver<KeyValuePair<string, object?>>
    {
        public void OnNext(KeyValuePair<string, object?> value)
        {
            switch (value.Key)
            {
                case SpecificationDiagnosticEvents.SpecificationEvaluated
                    when value.Value is SpecificationEvaluatedData data:
                    LogSpecificationEvaluated(data);
                    break;

                case SpecificationDiagnosticEvents.SpecificationFailed
                    when value.Value is SpecificationFailedData data:
                    LogSpecificationFailed(data);
                    break;

                case SpecificationDiagnosticEvents.CompositeSpecificationCreated
                    when value.Value is CompositeSpecificationCreatedData data:
                    LogCompositeCreated(data);
                    break;

                case SpecificationDiagnosticEvents.ExpressionCompiled
                    when value.Value is ExpressionCompiledData data:
                    LogExpressionCompiled(data);
                    break;
            }
        }

        private void LogSpecificationEvaluated(SpecificationEvaluatedData data)
        {
            logger.LogSpecificationEvaluated(
                data.SpecificationType,
                data.EntityType,
                data.ElapsedMilliseconds,
                data.IncludeCount,
                data.IgnoredQueryFilters,
                data.HasOrdering,
                data.HasPagination);
        }

        private void LogSpecificationFailed(SpecificationFailedData data)
        {
            logger.LogSpecificationFailed(
                data.SpecificationType,
                data.EntityType,
                data.ErrorMessage,
                data.ExceptionType);
        }

        private void LogCompositeCreated(CompositeSpecificationCreatedData data)
        {
            if (data.RightSpecificationType is not null)
            {
                logger.LogCompositeCreatedBinary(
                    data.CompositeType,
                    data.EntityType,
                    data.LeftSpecificationType,
                    data.RightSpecificationType);
            }
            else
            {
                logger.LogCompositeCreatedUnary(
                    data.CompositeType,
                    data.EntityType,
                    data.LeftSpecificationType);
            }
        }

        private void LogExpressionCompiled(ExpressionCompiledData data)
        {
            logger.LogExpressionCompiled(
                data.SpecificationType,
                data.EntityType,
                data.CompilationMilliseconds);
        }

        public void OnCompleted() { }
        public void OnError(Exception error) { }
    }

    [LoggerMessage(LogLevel.Error, "Error in specification diagnostic listener")]
    partial void LogDiagnosticError(Exception error);
}

internal static partial class SpecificationEventObserverLog
{
    [LoggerMessage(LogLevel.Debug,
        "Specification {SpecificationType}<{EntityType}> evaluated in {ElapsedMs:F2}ms " +
        "[Includes: {IncludeCount}, IgnoreFilters: {IgnoreFilters}, Ordered: {HasOrdering}, Paginated: {HasPagination}]")]
    public static partial void LogSpecificationEvaluated(
        this ILogger logger,
        string specificationType,
        string entityType,
        double elapsedMs,
        int includeCount,
        bool ignoreFilters,
        bool hasOrdering,
        bool hasPagination);

    [LoggerMessage(LogLevel.Warning,
        "Specification {SpecificationType}<{EntityType}> failed: {ErrorMessage} ({ExceptionType})")]
    public static partial void LogSpecificationFailed(
        this ILogger logger,
        string specificationType,
        string entityType,
        string errorMessage,
        string exceptionType);

    [LoggerMessage(LogLevel.Trace,
        "Created {CompositeType}<{EntityType}> from {Left} and {Right}")]
    public static partial void LogCompositeCreatedBinary(
        this ILogger logger,
        string compositeType,
        string entityType,
        string left,
        string? right);

    [LoggerMessage(LogLevel.Trace,
        "Created {CompositeType}<{EntityType}> from {Left}")]
    public static partial void LogCompositeCreatedUnary(
        this ILogger logger,
        string compositeType,
        string entityType,
        string left);

    [LoggerMessage(LogLevel.Trace,
        "Compiled expression for {SpecificationType}<{EntityType}> in {CompilationMs:F2}ms")]
    public static partial void LogExpressionCompiled(
        this ILogger logger,
        string specificationType,
        string entityType,
        double compilationMs);
}
