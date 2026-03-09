using System.Collections.Concurrent;
using System.Reflection;
using FamilyHub.Api.Common.Infrastructure.Auth;
using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Api.Common.Modules;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using Mediator;

namespace FamilyHub.Api.Common.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that resolves the authenticated user and populates
/// UserId/FamilyId on commands/queries implementing <see cref="IRequireUser"/>
/// or <see cref="IRequireFamily"/>. Replaces the former FamilyMembershipBehavior.
///
/// Decision tree:
///   1. <see cref="IAnonymousOperation"/> → skip (no user resolution needed)
///   2. <see cref="IRequireFamily"/> → resolve user, validate family membership, populate UserId + FamilyId
///   3. <see cref="IRequireUser"/> → resolve user, populate UserId only
///   4. Otherwise → throw InvalidOperationException (developer error: missing marker interface)
///
/// Uses reflection to set init-only properties on record types. The PropertyInfo lookups
/// are cached per message type for performance.
/// Unauthenticated requests are skipped (they'll be rejected by [Authorize] attributes).
/// </summary>
[PipelinePriority(PipelinePriorities.UserResolution)]
public sealed class UserResolutionBehavior<TMessage, TResponse>(
    ICurrentUserContext currentUserContext)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> UserIdPropertyCache = new();
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> FamilyIdPropertyCache = new();

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        // 1. Skip anonymous operations (token-based, public, or pre-registration flows)
        if (message is IAnonymousOperation)
        {
            return await next(message, cancellationToken);
        }

        // Skip for unauthenticated requests (auth middleware handles rejection)
        if (!currentUserContext.IsAuthenticated)
        {
            return await next(message, cancellationToken);
        }

        // 2. IRequireFamily — resolve user, validate family, populate both UserId + FamilyId
        if (message is IRequireFamily)
        {
            var userInfo = await currentUserContext.GetCurrentUserAsync();

            if (userInfo.FamilyId is null)
            {
                throw new FamilyMembershipRequiredException();
            }

            SetProperty<UserId>(message, nameof(IRequireUser.UserId), userInfo.UserId);
            SetProperty<FamilyId>(message, nameof(IRequireFamily.FamilyId), userInfo.FamilyId.Value);

            return await next(message, cancellationToken);
        }

        // 3. IRequireUser — resolve user, populate UserId only
        if (message is IRequireUser)
        {
            var userInfo = await currentUserContext.GetCurrentUserAsync();

            SetProperty<UserId>(message, nameof(IRequireUser.UserId), userInfo.UserId);

            return await next(message, cancellationToken);
        }

        // 4. Fallback: all commands/queries must implement a marker interface
        throw new InvalidOperationException(
            $"Command/query {typeof(TMessage).Name} must implement IRequireFamily, IRequireUser, or IAnonymousOperation. " +
            "This is a developer error — add the appropriate interface to the message type.");
    }

    private static void SetProperty<TValue>(TMessage target, string propertyName, TValue value)
    {
        var cache = propertyName == nameof(IRequireFamily.FamilyId) ? FamilyIdPropertyCache : UserIdPropertyCache;
        var messageType = typeof(TMessage);

        var propertyInfo = cache.GetOrAdd(messageType, type =>
            type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance));

        if (propertyInfo is null)
        {
            throw new InvalidOperationException(
                $"Property '{propertyName}' not found on {typeof(TMessage).Name}. " +
                "Commands implementing IRequireUser/IRequireFamily must have matching properties.");
        }

        propertyInfo.SetValue(target, value);
    }
}
