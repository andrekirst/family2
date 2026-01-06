using Microsoft.AspNetCore.Authorization;

namespace FamilyHub.Modules.Auth.Infrastructure.Authorization;

/// <summary>
/// Authorization requirement that checks if the user has Owner or Admin role.
/// Role is retrieved from database, not from JWT claims.
/// </summary>
public sealed class RequireOwnerOrAdminRequirement : IAuthorizationRequirement;
