using FamilyHub.Api.Features.Family.Models;
using FamilyHub.Api.Features.Family.Services;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace FamilyHub.Api.Features.Family.GraphQL;

/// <summary>
/// GraphQL queries for family data
/// </summary>
public class FamilyQueries
{
    /// <summary>
    /// Get the current user's family
    /// </summary>
    [Authorize]
    public async Task<FamilyDto?> GetMyFamily(
        ClaimsPrincipal claimsPrincipal,
        [Service] FamilyService familyService,
        [Service] Auth.Services.AuthService authService)
    {
        var externalUserId = claimsPrincipal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(externalUserId))
        {
            return null;
        }

        var user = await authService.GetUserByExternalIdAsync(externalUserId);
        if (user?.FamilyId == null)
        {
            return null;
        }

        return await familyService.GetFamilyByIdAsync(user.FamilyId.Value);
    }

    /// <summary>
    /// Get all members of the current user's family
    /// </summary>
    [Authorize]
    public async Task<List<Auth.Models.UserDto>> GetMyFamilyMembers(
        ClaimsPrincipal claimsPrincipal,
        [Service] FamilyService familyService,
        [Service] Auth.Services.AuthService authService)
    {
        var externalUserId = claimsPrincipal.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var user = await authService.GetUserByExternalIdAsync(externalUserId);
        if (user?.FamilyId == null)
        {
            return new List<Auth.Models.UserDto>();
        }

        return await familyService.GetFamilyMembersAsync(user.FamilyId.Value);
    }
}
