using FamilyHub.Api.Common.Modules;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FamilyHub.Api.Common.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that calls IUnitOfWork.SaveChangesAsync() after the handler.
/// This removes the need for individual handlers to call SaveChangesAsync.
///
/// Handles DbUpdateConcurrencyException by converting it to a ConflictException,
/// providing clear feedback when optimistic concurrency conflicts occur.
/// </summary>
[PipelinePriority(PipelinePriorities.Transaction)]
public sealed class TransactionBehavior<TMessage, TResponse>(IUnitOfWork unitOfWork)
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

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var response = await next(message, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return response;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackAsync(cancellationToken);

            // Extract the entity type from the concurrency exception for a clear error message
            var entityType = ex.Entries.FirstOrDefault()?.Metadata.ClrType.Name ?? "Entity";
            throw new ConflictException(entityType);
        }
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
