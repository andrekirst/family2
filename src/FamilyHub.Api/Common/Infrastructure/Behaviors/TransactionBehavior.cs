using FamilyHub.Api.Common.Application;
using Mediator;

namespace FamilyHub.Api.Common.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that calls IUnitOfWork.SaveChangesAsync() after the handler.
/// This removes the need for individual handlers to call SaveChangesAsync.
/// </summary>
public sealed class TransactionBehavior<TMessage, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(message, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}
