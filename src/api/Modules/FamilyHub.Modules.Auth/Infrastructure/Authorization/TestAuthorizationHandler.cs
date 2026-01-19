using Microsoft.AspNetCore.Authorization;

namespace FamilyHub.Modules.Auth.Infrastructure.Authorization;

/// <summary>
/// Authorization handler that succeeds all requirements for E2E testing.
/// </summary>
/// <remarks>
/// <para>
/// SECURITY: This handler should ONLY be registered when TestMode is enabled.
/// It bypasses all authorization checks, allowing any request to pass.
/// </para>
/// <para>
/// In test mode, authentication is handled by <see cref="Services.HeaderBasedCurrentUserService"/>
/// which extracts user identity from HTTP headers. This handler complements that by
/// ensuring all authorization policies pass once the user identity is established.
/// </para>
/// </remarks>
public sealed class TestAuthorizationHandler : IAuthorizationHandler
{
    /// <summary>
    /// Handles authorization by succeeding all pending requirements.
    /// </summary>
    /// <param name="context">The authorization context.</param>
    /// <returns>A completed task.</returns>
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        foreach (var requirement in context.PendingRequirements.ToList())
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
