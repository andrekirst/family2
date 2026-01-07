using Microsoft.AspNetCore.Authorization;

namespace FamilyHub.Modules.Auth.Infrastructure.Authorization;

/// <summary>
/// Authorization requirement that checks if the user has Owner role.
/// Role is retrieved from JWT claims (already validated during authentication).
/// </summary>
public sealed class RequireOwnerRequirement : IAuthorizationRequirement;
