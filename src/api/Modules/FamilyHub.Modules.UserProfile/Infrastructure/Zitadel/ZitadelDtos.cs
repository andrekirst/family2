using System.Text.Json.Serialization;

namespace FamilyHub.Modules.UserProfile.Infrastructure.Zitadel;

/// <summary>
/// Represents a user profile retrieved from Zitadel.
/// </summary>
public sealed record ZitadelUserProfile
{
    /// <summary>
    /// The Zitadel user ID.
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// The user's display name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// The user's first name (optional).
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// The user's last name (optional).
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// The user's nickname (optional).
    /// </summary>
    public string? NickName { get; init; }

    /// <summary>
    /// The user's preferred language (optional).
    /// </summary>
    public string? PreferredLanguage { get; init; }
}

/// <summary>
/// Response from Zitadel Management API when getting a user.
/// </summary>
internal sealed record ZitadelUserResponse
{
    [JsonPropertyName("user")]
    public ZitadelUser? User { get; init; }
}

/// <summary>
/// User object in Zitadel API response.
/// </summary>
internal sealed record ZitadelUser
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("human")]
    public ZitadelHuman? Human { get; init; }

    [JsonPropertyName("changeDate")]
    public DateTime? ChangeDate { get; init; }
}

/// <summary>
/// Human user details in Zitadel API response.
/// </summary>
internal sealed record ZitadelHuman
{
    [JsonPropertyName("profile")]
    public ZitadelProfile? Profile { get; init; }

    [JsonPropertyName("email")]
    public ZitadelEmail? Email { get; init; }
}

/// <summary>
/// Profile details in Zitadel API response.
/// </summary>
internal sealed record ZitadelProfile
{
    [JsonPropertyName("firstName")]
    public string? FirstName { get; init; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; init; }

    [JsonPropertyName("nickName")]
    public string? NickName { get; init; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; init; }

    [JsonPropertyName("preferredLanguage")]
    public string? PreferredLanguage { get; init; }
}

/// <summary>
/// Email details in Zitadel API response.
/// </summary>
internal sealed record ZitadelEmail
{
    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("isVerified")]
    public bool IsVerified { get; init; }
}

/// <summary>
/// Request body for updating a user profile in Zitadel.
/// </summary>
internal sealed record ZitadelUpdateProfileRequest
{
    [JsonPropertyName("displayName")]
    public required string DisplayName { get; init; }
}
