using FamilyHub.Api.Common.Modules;
using FamilyHub.Common.Application;
using Mediator;

namespace FamilyHub.Api.Common.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that detects streaming results and sets a diagnostic flag
/// so downstream <see cref="LoggingBehavior{TMessage,TResponse}"/> skips serializing
/// stream content in log output.
/// </summary>
[PipelinePriority(PipelinePriorities.Streaming)]
public sealed class StreamingBehavior<TMessage, TResponse>(
    IHttpContextAccessor httpContextAccessor)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    internal const string IsStreamingKey = "FamilyHub.IsStreaming";

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(message, cancellationToken);

        // If the result is a streaming result (or a Result<T> wrapping one),
        // set a flag on HttpContext so LoggingBehavior can skip content serialization.
        if (IsStreamingResponse(response))
        {
            httpContextAccessor.HttpContext?.Items.TryAdd(IsStreamingKey, true);
        }

        return response;
    }

    private static bool IsStreamingResponse(TResponse response)
    {
        if (response is IStreamableResult)
            return true;

        // Check if it's a Result<T> where T is IStreamableResult
        if (response is Result<IStreamableResult> { IsSuccess: true })
            return true;

        // Use reflection for Result<ConcreteStreamableResult> cases
        var responseType = typeof(TResponse);
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var innerType = responseType.GetGenericArguments()[0];
            if (typeof(IStreamableResult).IsAssignableFrom(innerType))
                return true;
        }

        return false;
    }
}
