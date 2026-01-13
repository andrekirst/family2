using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Application.Services;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that enriches the request context with the authenticated user's information.
/// Executes after LoggingBehavior and before AuthorizationBehavior in the pipeline.
/// Skips enrichment for public queries (IPublicQuery marker).
/// Loads full User aggregate from database for authenticated requests (IRequireAuthentication marker).
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
/// <param name="currentUserService">Service for extracting current user from HTTP context.</param>
/// <param name="userRepository">Repository for user data access.</param>
/// <param name="userContext">Scoped user context to populate.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class UserContextEnrichmentBehavior<TRequest, TResponse>(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IUserContext userContext,
    ILogger<UserContextEnrichmentBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Skip enrichment for public queries
        if (request is IPublicQuery)
        {
            LogSkippingPublicQuery(typeof(TRequest).Name);
            return await next(cancellationToken);
        }

        // Only enrich if request requires authentication
        if (request is not IRequireAuthentication)
        {
            return await next(cancellationToken);
        }

        // 1. Extract UserId from JWT claims
        var userId = await currentUserService.GetUserIdAsync(cancellationToken);

        // 2. Load full User aggregate from database
        var user = await userRepository.GetByIdAsync(userId, cancellationToken) ?? throw new UnauthorizedAccessException(
                $"User with ID '{userId.Value}' not found in database. " +
                "The user may have been deleted or the JWT token is stale.");

        // 4. Populate scoped IUserContext service
        ((UserContextService)userContext).SetUser(user);

        LogUserContextEnriched(typeof(TRequest).Name, userId.Value);

        // 5. Continue pipeline with enriched context
        return await next(cancellationToken);
    }

    [LoggerMessage(LogLevel.Debug, "User context enriched for {requestName}: UserId={userId}")]
    partial void LogUserContextEnriched(string requestName, Guid userId);

    [LoggerMessage(LogLevel.Debug, "Skipping user context enrichment for public query: {requestName}")]
    partial void LogSkippingPublicQuery(string requestName);
}
