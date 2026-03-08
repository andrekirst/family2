using FamilyHub.Api.Common.Modules;
using FamilyHub.Common.Application;
using Mediator;

namespace FamilyHub.Api.Common.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that calls IUnitOfWork.SaveChangesAsync() after the handler.
/// This removes the need for individual handlers to call SaveChangesAsync.
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
        catch
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
