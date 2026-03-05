using System.Net.Http.Headers;
using System.Text.Json;
using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Features.Auth.Domain.Entities;
using FamilyHub.Api.Features.Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using DomainEmail = FamilyHub.Common.Domain.ValueObjects.Email;
using DomainExternalUserId = FamilyHub.Common.Domain.ValueObjects.ExternalUserId;

namespace FamilyHub.Api.Common.Development;

/// <summary>
/// Seeds the application database with users from Keycloak in development environments.
/// Queries Keycloak's admin API to discover test users and their auto-generated UUIDs,
/// then creates matching User records in the database so developers don't need to go
/// through the OAuth registration flow.
/// </summary>
public sealed class DevDataSeeder(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<DevDataSeeder> logger) : IHostedService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var (baseUrl, realmName) = ParseKeycloakAuthority();
        if (baseUrl is null || realmName is null)
        {
            logger.LogWarning("DevDataSeeder: Could not parse Keycloak authority — skipping user seeding");
            return;
        }

        // Keycloak may still be starting; retry a few times
        const int maxRetries = 10;
        const int delayMs = 3000;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await SeedUsersAsync(baseUrl, realmName, cancellationToken);
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                logger.LogDebug(ex, "DevDataSeeder: Attempt {Attempt}/{Max} failed, retrying in {Delay}ms",
                    attempt, maxRetries, delayMs);
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        logger.LogWarning("DevDataSeeder: All attempts to seed users failed — users will be created on first OAuth login instead");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedUsersAsync(string baseUrl, string realmName, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var adminToken = await GetAdminTokenAsync(baseUrl, ct);
        var keycloakUsers = await GetRealmUsersAsync(baseUrl, realmName, adminToken, ct);

        if (keycloakUsers.Count == 0)
        {
            logger.LogInformation("DevDataSeeder: No users found in Keycloak realm {Realm}", realmName);
            return;
        }

        var seeded = 0;
        var updated = 0;

        foreach (var kcUser in keycloakUsers)
        {
            if (string.IsNullOrWhiteSpace(kcUser.Email))
            {
                continue;
            }

            var email = DomainEmail.From(kcUser.Email);
            var existingUser = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == email, ct);

            if (existingUser is not null)
            {
                // Re-link external ID if realm was recreated (new Keycloak UUIDs)
                var currentExternalId = DomainExternalUserId.From(kcUser.Id);
                if (existingUser.ExternalUserId != currentExternalId)
                {
                    existingUser.UpdateExternalId(currentExternalId);
                    existingUser.ClearDomainEvents();
                    updated++;
                }
                continue;
            }

            var displayName = $"{kcUser.FirstName} {kcUser.LastName}".Trim();
            if (displayName.Length < 2)
            {
                displayName = kcUser.Username ?? kcUser.Email;
            }

            var user = User.Register(
                email,
                UserName.From(displayName),
                DomainExternalUserId.From(kcUser.Id),
                kcUser.EmailVerified,
                kcUser.Username);

            user.ClearDomainEvents();
            dbContext.Users.Add(user);
            seeded++;
        }

        if (seeded > 0 || updated > 0)
        {
            await dbContext.SaveChangesAsync(ct);
        }

        logger.LogInformation(
            "DevDataSeeder: {Seeded} user(s) seeded, {Updated} user(s) updated from Keycloak realm {Realm}",
            seeded, updated, realmName);
    }

    private async Task<string> GetAdminTokenAsync(string baseUrl, CancellationToken ct)
    {
        using var http = new HttpClient();
        var tokenUrl = $"{baseUrl}/realms/master/protocol/openid-connect/token";

        var response = await http.PostAsync(tokenUrl, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = "admin-cli",
            ["grant_type"] = "password",
            ["username"] = "admin",
            ["password"] = "admin",
        }), ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
        return json.GetProperty("access_token").GetString()
            ?? throw new InvalidOperationException("No access_token in Keycloak admin token response");
    }

    private async Task<List<KeycloakUser>> GetRealmUsersAsync(
        string baseUrl, string realmName, string adminToken, CancellationToken ct)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var usersUrl = $"{baseUrl}/admin/realms/{realmName}/users?max=100";
        var response = await http.GetAsync(usersUrl, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<KeycloakUser>>(JsonOptions, ct) ?? [];
    }

    private (string? BaseUrl, string? RealmName) ParseKeycloakAuthority()
    {
        var authority = configuration["Keycloak:Authority"];
        if (string.IsNullOrWhiteSpace(authority))
        {
            return (null, null);
        }

        // Authority format: http://keycloak:8080/realms/FamilyHub-{env}
        const string realmsSegment = "/realms/";
        var realmsIndex = authority.IndexOf(realmsSegment, StringComparison.Ordinal);
        if (realmsIndex < 0)
        {
            return (null, null);
        }

        var baseUrl = authority[..realmsIndex];
        var realmName = authority[(realmsIndex + realmsSegment.Length)..];

        return (baseUrl, realmName);
    }

    private sealed record KeycloakUser(
        string Id,
        string? Username,
        string Email,
        string? FirstName,
        string? LastName,
        bool EmailVerified);
}
