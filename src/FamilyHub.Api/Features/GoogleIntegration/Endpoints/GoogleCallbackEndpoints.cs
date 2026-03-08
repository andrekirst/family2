using FamilyHub.Api.Common.Configuration;
using FamilyHub.Api.Features.GoogleIntegration.Application.Commands.LinkGoogleAccount;
using FamilyHub.Common.Application;
using Microsoft.Extensions.Options;

namespace FamilyHub.Api.Features.GoogleIntegration.Endpoints;

/// <summary>
/// Minimal API endpoints for Google OAuth callback.
/// Handles the OAuth redirect from Google and delegates to the LinkGoogleAccount command.
/// </summary>
public static class GoogleCallbackEndpoints
{
    public static async Task<IResult> Callback(
        string? code,
        string? state,
        string? error,
        ICommandBus commandBus,
        IOptions<AppOptions> appOptions,
        CancellationToken cancellationToken)
    {
        var frontendUrl = appOptions.Value.FrontendUrl;

        if (!string.IsNullOrEmpty(error))
        {
            return Results.Redirect($"{frontendUrl}/settings?google_error={Uri.EscapeDataString(error)}");
        }

        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
        {
            return Results.Redirect($"{frontendUrl}/settings?google_error=missing_parameters");
        }

        var command = new LinkGoogleAccountCommand(code, state);
        var result = await commandBus.SendAsync(command, cancellationToken);

        return result.Success
            ? Results.Redirect($"{frontendUrl}/settings?google_linked=true")
            : Results.Redirect($"{frontendUrl}/settings?google_error={Uri.EscapeDataString(result.Error ?? "unknown")}");
    }
}
