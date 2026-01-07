using Microsoft.AspNetCore.Authorization;

namespace FamilyHub.Modules.Auth.Infrastructure.Authorization;

/// <summary>
/// Authorization requirement that checks if the user has Admin role.
/// Role is retrieved from JWT claims (already validated during authentication).
/// </summary>
public sealed class RequireAdminRequirement : IAuthorizationRequirement;
