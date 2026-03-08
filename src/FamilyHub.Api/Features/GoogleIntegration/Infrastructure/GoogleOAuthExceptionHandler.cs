using FamilyHub.Api.Common.Configuration;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;

namespace FamilyHub.Api.Features.GoogleIntegration.Infrastructure;

/// <summary>
/// Exception handler for Google OAuth callback flow.
/// Catches exceptions from the /api/google path and redirects to the frontend
/// with an error query parameter instead of returning a 500 error.
/// </summary>
public sealed class GoogleOAuthExceptionHandler(
    IOptions<AppOptions> appOptions,
    ILogger<GoogleOAuthExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (!httpContext.Request.Path.StartsWithSegments("/api/google"))
        {
            return false;
        }

        logger.LogError(exception, "Google OAuth callback failed");

        var frontendUrl = appOptions.Value.FrontendUrl;
        var errorMessage = Uri.EscapeDataString(exception.Message);

        httpContext.Response.Redirect($"{frontendUrl}/settings?google_error={errorMessage}");
        await Task.CompletedTask;
        return true;
    }
}
