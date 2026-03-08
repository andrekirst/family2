namespace FamilyHub.Api.Common.Infrastructure;

/// <summary>
/// JWT claim name constants organized by specification category.
/// </summary>
public static class ClaimNames
{
    /// <summary>
    /// Standard OIDC/OAuth 2.0 claims as defined in the OpenID Connect Core spec.
    /// </summary>
    public static class Standard
    {
        public const string Sub = "sub";
        public const string Email = "email";
        public const string Name = "name";
        public const string EmailVerified = "email_verified";
        public const string PreferredUsername = "preferred_username";
    }

    /// <summary>
    /// Keycloak-specific claims not part of the OIDC standard.
    /// </summary>
    public static class Keycloak
    {
        public const string RealmAccess = "realm_access";
        public const string ResourceAccess = "resource_access";
    }
}
