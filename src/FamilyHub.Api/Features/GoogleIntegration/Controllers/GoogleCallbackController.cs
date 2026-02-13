using FamilyHub.Common.Application;
using FamilyHub.Api.Features.GoogleIntegration.Application.Commands.LinkGoogleAccount;
using Microsoft.AspNetCore.Mvc;

namespace FamilyHub.Api.Features.GoogleIntegration.Controllers;

[ApiController]
[Route("api/google")]
public class GoogleCallbackController(
    ICommandBus commandBus,
    IConfiguration configuration) : ControllerBase
{
    [HttpGet("callback")]
    public async Task<IActionResult> Callback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        CancellationToken cancellationToken)
    {
        var frontendUrl = configuration["App:FrontendUrl"] ?? "http://localhost:4200";

        if (!string.IsNullOrEmpty(error))
            return Redirect($"{frontendUrl}/settings?google_error={Uri.EscapeDataString(error)}");

        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            return Redirect($"{frontendUrl}/settings?google_error=missing_parameters");

        try
        {
            var command = new LinkGoogleAccountCommand(code, state);
            var result = await commandBus.SendAsync(command, cancellationToken);

            return result.Success
                ? Redirect($"{frontendUrl}/settings?google_linked=true")
                : Redirect($"{frontendUrl}/settings?google_error={Uri.EscapeDataString(result.Error ?? "unknown")}");
        }
        catch (Exception ex)
        {
            return Redirect($"{frontendUrl}/settings?google_error={Uri.EscapeDataString(ex.Message)}");
        }
    }
}
