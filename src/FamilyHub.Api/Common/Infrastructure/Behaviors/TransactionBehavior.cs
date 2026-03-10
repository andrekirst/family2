using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Common.Modules;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Common.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that wraps command handlers in a database transaction.
/// Uses CreateExecutionStrategy().ExecuteAsync() to be compatible with
/// NpgsqlRetryingExecutionStrategy (EnableRetryOnFailure).
///
/// Handles DbUpdateConcurrencyException by converting it to a ConflictException,
/// providing clear feedback when optimistic concurrency conflicts occur.
/// </summary>
[PipelinePriority(PipelinePriorities.Transaction)]
public sealed class TransactionBehavior<TMessage, TResponse>(IUnitOfWork unitOfWork, AppDbContext dbContext)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        // Skip transactions for read-only queries — no state to persist
        if (message is IReadOnlyQuery<TResponse>)
        {
            return await next(message, cancellationToken);
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async ct =>
        {
            await unitOfWork.BeginTransactionAsync(ct);
            try
            {
                var response = await next(message, ct);
                await unitOfWork.SaveChangesAsync(ct);
                await unitOfWork.CommitAsync(ct);
                return response;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await unitOfWork.RollbackAsync(ct);

                // Extract the entity type from the concurrency exception for a clear error message
                var entityType = ex.Entries.FirstOrDefault()?.Metadata.ClrType.Name ?? "Entity";
                throw new ConflictException(entityType);
            }
            catch
            {
                await unitOfWork.RollbackAsync(ct);
                throw;
            }
        }, cancellationToken);
    }
}
