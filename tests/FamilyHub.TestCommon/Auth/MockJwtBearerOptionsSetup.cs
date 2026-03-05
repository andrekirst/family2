using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace FamilyHub.TestCommon.Auth;

/// <summary>
/// PostConfigure delegate that overrides JwtBearerOptions to use in-memory RSA keys
/// instead of fetching from Keycloak's OIDC discovery endpoint.
/// Register via: <c>services.PostConfigure&lt;JwtBearerOptions&gt;(JwtBearerDefaults.AuthenticationScheme, MockJwtBearerOptionsSetup.Configure)</c>
/// </summary>
public static class MockJwtBearerOptionsSetup
{
    public static void Configure(JwtBearerOptions options)
    {
        // Disable HTTP-based OIDC discovery by providing an empty in-memory configuration
        options.Configuration = new OpenIdConnectConfiguration();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = MockJwtTokenGenerator.Issuer,
            ValidateAudience = true,
            ValidAudience = MockJwtTokenGenerator.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = MockJwtTokenGenerator.SecurityKey,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    }
}
