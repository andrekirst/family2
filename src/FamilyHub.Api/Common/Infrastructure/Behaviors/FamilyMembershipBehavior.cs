using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace FamilyHub.Api.Common.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that enforces family membership before any command/query proceeds.
/// Runs at priority 350 (after Validation, before QueryAsNoTracking).
///
/// Decision tree:
///   1. If message implements IIgnoreFamilyMembership → skip check entirely
///   2. If message implements IFamilyScoped → verify user is a member of THAT specific family
///   3. Otherwise (fallback) → throw InvalidOperationException (all commands must be tagged)
///
/// Unauthenticated requests are skipped (they'll be rejected by [Authorize] attributes).
/// </summary>
[PipelinePriority(PipelinePriorities.FamilyMembership)]
public sealed class FamilyMembershipBehavior<TMessage, TResponse>(
    IHttpContextAccessor httpContextAccessor,
    IUserRepository userRepository)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        // 1. Skip if message opts out of family membership check
        if (message is IIgnoreFamilyMembership)
        {
            return await next(message, cancellationToken);
        }

        // Skip for unauthenticated requests (auth middleware handles rejection)
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User.Identity?.IsAuthenticated != true)
        {
            return await next(message, cancellationToken);
        }

        var externalUserIdString = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(externalUserIdString))
        {
            return await next(message, cancellationToken);
        }

        var externalUserId = ExternalUserId.From(externalUserIdString);
        var user = await userRepository.GetByExternalIdAsync(externalUserId, cancellationToken);

        if (user is null)
        {
            // User not registered yet (e.g., during RegisterUser flow) — skip
            return await next(message, cancellationToken);
        }

        // 2. If message targets a specific family, verify membership in THAT family
        if (message is IFamilyScoped familyScoped)
        {
            if (user.FamilyId is null || user.FamilyId != familyScoped.FamilyId)
            {
                throw new FamilyMembershipRequiredException();
            }

            return await next(message, cancellationToken);
        }

        // 3. Fallback: all commands/queries must implement IFamilyScoped or IIgnoreFamilyMembership
        throw new InvalidOperationException(
            $"Command/query {typeof(TMessage).Name} must implement IFamilyScoped or IIgnoreFamilyMembership. " +
            "This is a developer error — add the appropriate interface to the message type.");
    }
}
