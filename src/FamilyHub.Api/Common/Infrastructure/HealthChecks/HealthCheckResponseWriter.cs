using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FamilyHub.Api.Common.Infrastructure.HealthChecks;

/// <summary>
/// Writes a detailed JSON response for the /health/auth endpoint showing per-check status.
/// </summary>
public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static async Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var checks = report.Entries.ToDictionary(
            entry => entry.Key,
            entry => new
            {
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description ?? entry.Value.Status.ToString()
            });

        var response = new
        {
            status = report.Status.ToString(),
            checks
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
