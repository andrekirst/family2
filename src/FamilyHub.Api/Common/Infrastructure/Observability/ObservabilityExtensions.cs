using System.Diagnostics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FamilyHub.Api.Common.Infrastructure.Observability;

/// <summary>
/// Configures OpenTelemetry traces, metrics, and logging with OTLP export.
/// Custom ActivitySource for domain operations enables fine-grained tracing
/// of command handling, event publishing, and pipeline behaviors.
/// </summary>
public static class ObservabilityExtensions
{
    public const string ServiceName = "FamilyHub.Api";

    /// <summary>
    /// Custom ActivitySource for domain-level tracing (commands, events, outbox).
    /// Use this to create spans for business operations beyond HTTP/DB instrumentation.
    /// </summary>
    public static readonly ActivitySource DomainActivitySource = new(ServiceName);

    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: ServiceName,
                    serviceVersion: typeof(ObservabilityExtensions).Assembly
                        .GetName().Version?.ToString() ?? "1.0.0"))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = httpContext =>
                        !httpContext.Request.Path.StartsWithSegments("/health");
                })
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddSource(ServiceName)
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter("FamilyHub.Api.Commands")
                .AddMeter("FamilyHub.Api.Events")
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                }));

        return services;
    }
}
