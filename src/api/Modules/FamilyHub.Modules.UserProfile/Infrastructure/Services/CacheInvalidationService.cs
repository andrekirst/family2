using FamilyHub.Infrastructure.Messaging;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.UserProfile.Infrastructure.Services;

/// <summary>
/// Cache invalidation service implementation for the UserProfile module.
/// Uses Redis subscription publisher for cache invalidation signals.
/// </summary>
public sealed partial class CacheInvalidationService : ICacheInvalidationService
{
    private readonly IRedisSubscriptionPublisher _redisPublisher;
    private readonly ILogger<CacheInvalidationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheInvalidationService"/> class.
    /// </summary>
    /// <param name="redisPublisher">The Redis subscription publisher for cache invalidation signals.</param>
    /// <param name="logger">Logger for structured logging.</param>
    public CacheInvalidationService(
        IRedisSubscriptionPublisher redisPublisher,
        ILogger<CacheInvalidationService> logger)
    {
        _redisPublisher = redisPublisher;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task InvalidateFamilyMembersCacheAsync(
        FamilyId familyId,
        CancellationToken cancellationToken = default)
    {
        LogFamilyMembersCacheInvalidation(familyId.Value);

        // Publish cache invalidation signal via Redis
        // Consumers listening to this topic will invalidate their local caches
        await _redisPublisher.PublishAsync(
            $"cache-invalidation:family-members:{familyId.Value}",
            new CacheInvalidationMessage("family-members", familyId.Value.ToString()),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task InvalidateUserProfileCacheAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        LogUserProfileCacheInvalidation(userId.Value);

        await _redisPublisher.PublishAsync(
            $"cache-invalidation:user-profile:{userId.Value}",
            new CacheInvalidationMessage("user-profile", userId.Value.ToString()),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task InvalidateAllUserCachesAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        LogAllUserCachesInvalidation(userId.Value);

        // Invalidate all user-related caches
        await InvalidateUserProfileCacheAsync(userId, cancellationToken);

        // Publish a general user cache invalidation for any other listeners
        await _redisPublisher.PublishAsync(
            $"cache-invalidation:user:{userId.Value}",
            new CacheInvalidationMessage("user-all", userId.Value.ToString()),
            cancellationToken);
    }

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Invalidating family members cache for FamilyId={FamilyId}")]
    private partial void LogFamilyMembersCacheInvalidation(Guid familyId);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Invalidating user profile cache for UserId={UserId}")]
    private partial void LogUserProfileCacheInvalidation(Guid userId);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Invalidating all caches for UserId={UserId}")]
    private partial void LogAllUserCachesInvalidation(Guid userId);
}

/// <summary>
/// Message payload for cache invalidation signals.
/// </summary>
/// <param name="CacheType">The type of cache being invalidated.</param>
/// <param name="Key">The key or identifier of the invalidated data.</param>
internal sealed record CacheInvalidationMessage(string CacheType, string Key);
