using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace FamilyHub.Api.Common.Infrastructure.Resilience;

/// <summary>
/// Extension methods for configuring Polly resilience pipelines.
/// Provides named pipelines for database and HTTP client resilience.
/// </summary>
public static class ResilienceExtensions
{
    /// <summary>
    /// Registers named Polly resilience pipelines for database and HTTP client operations.
    /// </summary>
    public static IServiceCollection AddResiliencePipelines(this IServiceCollection services)
    {
        services.AddResiliencePipeline(ResiliencePipelineKeys.Database, builder =>
        {
            builder.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<NpgsqlException>(ex => ex.IsTransient)
                    .Handle<TimeoutException>(),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            });
        });

        services.AddResiliencePipeline(ResiliencePipelineKeys.HttpClient, builder =>
        {
            // Total timeout: 30s for the entire pipeline execution
            builder.AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(30)
            });

            // Retry: 3 attempts with exponential backoff
            builder.AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutRejectedException>(),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            });

            // Circuit breaker: break after 5 failures in 30s, stay open for 30s
            builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutRejectedException>(),
                FailureRatio = 0.5,
                MinimumThroughput = 5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(30)
            });

            // Per-attempt timeout: 10s for each individual attempt
            builder.AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(10)
            });
        });

        return services;
    }
}
