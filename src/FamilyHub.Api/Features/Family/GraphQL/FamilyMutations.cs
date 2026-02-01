using FamilyHub.Api.Features.Family.Models;
using FamilyHub.Api.Features.Family.Services;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace FamilyHub.Api.Features.Family.GraphQL;

/// <summary>
/// GraphQL mutations for family management operations
/// </summary>
public class FamilyMutations
{
    /// <summary>
    /// Create a new family with the current user as owner
    /// </summary>
    [Authorize]
    public async Task<FamilyDto> CreateFamily(
        CreateFamilyRequest input,
        ClaimsPrincipal claimsPrincipal,
        [Service] FamilyService familyService,
        [Service] Auth.Services.AuthService authService)
    {
        // Get current user ID from JWT
        var externalUserId = claimsPrincipal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var user = await authService.GetUserByExternalIdAsync(externalUserId)
            ?? throw new UnauthorizedAccessException("User not found");

        // Create family
        return await familyService.CreateFamilyAsync(input, user.Id);
    }
}
