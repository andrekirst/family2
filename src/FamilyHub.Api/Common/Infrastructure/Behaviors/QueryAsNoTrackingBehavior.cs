using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Common.Modules;
using FamilyHub.Common.Application;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Common.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that sets the DbContext change tracker to NoTracking
/// for read-only queries. This improves performance by avoiding the overhead
/// of change tracking for queries that don't modify data.
/// </summary>
[PipelinePriority(PipelinePriorities.QueryAsNoTracking)]
public sealed class QueryAsNoTrackingBehavior<TMessage, TResponse>(AppDbContext dbContext)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (message is IReadOnlyQuery<TResponse>)
        {
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        return next(message, cancellationToken);
    }
}
