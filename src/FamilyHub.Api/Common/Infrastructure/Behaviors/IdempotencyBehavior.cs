using FamilyHub.Api.Common.Modules;
using FamilyHub.Common.Application;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FamilyHub.Api.Common.Database;

namespace FamilyHub.Api.Common.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that enforces idempotency for command operations.
/// When a client sends X-Idempotency-Key header, the behavior:
///   1. Checks if this key has been processed before
///   2. If yes, returns the cached result (no duplicate side effects)
///   3. If no, executes the command and caches the result
///
/// Priority 350: after validation (300), before transaction (400).
/// Only applies to commands (not queries) that have an idempotency key.
/// </summary>
[PipelinePriority(PipelinePriorities.Idempotency)]
public sealed class IdempotencyBehavior<TMessage, TResponse>(
    IHttpContextAccessor httpContextAccessor,
    AppDbContext dbContext,
    TimeProvider timeProvider,
    ILogger<IdempotencyBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private const string IdempotencyKeyHeader = "X-Idempotency-Key";

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        // Skip for queries — idempotency only matters for state-changing operations
        if (message is IReadOnlyQuery<TResponse>)
        {
            return await next(message, cancellationToken);
        }

        // Check for idempotency key in HTTP headers
        var idempotencyKey = httpContextAccessor.HttpContext?.Request.Headers[IdempotencyKeyHeader].FirstOrDefault();
        if (string.IsNullOrEmpty(idempotencyKey))
        {
            return await next(message, cancellationToken);
        }

        // Hash the key for consistent storage
        var keyHash = ComputeHash(idempotencyKey);

        // Check for existing result
        var existing = await dbContext.Set<IdempotencyRecord>()
            .FirstOrDefaultAsync(r => r.KeyHash == keyHash, cancellationToken);

        if (existing is not null)
        {
            logger.LogInformation("Idempotent request detected for key {KeyHash}, returning cached result", keyHash);

            if (existing.ResultJson is not null)
            {
                return JsonSerializer.Deserialize<TResponse>(existing.ResultJson)!;
            }

            return default!;
        }

        // Execute the command
        var response = await next(message, cancellationToken);

        // Cache the result
        var record = new IdempotencyRecord
        {
            KeyHash = keyHash,
            ResultJson = JsonSerializer.Serialize(response),
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            ExpiresAt = timeProvider.GetUtcNow().UtcDateTime.AddHours(24)
        };

        dbContext.Set<IdempotencyRecord>().Add(record);
        // Note: SaveChanges is called by TransactionBehavior (runs after this)

        return response;
    }

    private static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }
}

/// <summary>
/// Database record for idempotency key deduplication.
/// Stored in the idempotency_keys table, cleaned up by Hangfire after 24 hours.
/// </summary>
public sealed class IdempotencyRecord
{
    public required string KeyHash { get; init; }
    public string? ResultJson { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
}
