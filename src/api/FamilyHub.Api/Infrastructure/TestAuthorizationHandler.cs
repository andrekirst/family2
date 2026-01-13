using Microsoft.AspNetCore.Authorization;

namespace FamilyHub.Api.Infrastructure;

/// <summary>
/// Authorization handler for Test environment that always succeeds.
/// Used for k6 performance testing without real JWT tokens.
/// </summary>
/// <remarks>
/// This handler is ONLY registered when ASPNETCORE_ENVIRONMENT=Test.
/// It bypasses all authorization requirements, allowing k6 tests to
/// test authenticated GraphQL endpoints using the X-Test-User-Id header.
/// </remarks>
public sealed class TestAuthorizationHandler : IAuthorizationHandler
{
    /// <inheritdoc />
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        foreach (var requirement in context.PendingRequirements.ToList())
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
