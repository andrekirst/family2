using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace FamilyHub.Api.Common.Infrastructure.RateLimiting;

/// <summary>
/// Configures per-endpoint rate limiting policies using .NET's built-in RateLimiter middleware.
/// Policies partition by authenticated user ID when available, falling back to client IP address.
/// </summary>
public static class RateLimitingExtensions
{
    public const string GraphqlQueryPolicy = "graphql-query";
    public const string GraphqlMutationPolicy = "graphql-mutation";
    public const string EmailSendingPolicy = "email-sending";
    public const string GlobalPolicy = "global";

    public static IServiceCollection AddApplicationRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // GraphQL queries: 100 requests per minute per user
            options.AddPolicy(GraphqlQueryPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ResolvePartitionKey(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // GraphQL mutations: 30 requests per minute per user
            options.AddPolicy(GraphqlMutationPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ResolvePartitionKey(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // Email sending: 5 requests per minute per user
            options.AddPolicy(EmailSendingPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ResolvePartitionKey(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // Global: 200 requests per minute per IP
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ResolveIpAddress(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 200,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));
        });

        return services;
    }

    /// <summary>
    /// Resolves partition key: authenticated user's "sub" claim if available, otherwise client IP.
    /// </summary>
    private static string ResolvePartitionKey(HttpContext context)
    {
        var userId = context.User.FindFirstValue("sub");
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        return $"ip:{ResolveIpAddress(context)}";
    }

    /// <summary>
    /// Resolves the client IP address, respecting X-Forwarded-For when behind a reverse proxy.
    /// </summary>
    private static string ResolveIpAddress(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
