using System.Text.Json.Serialization;

namespace FamilyHub.Api.Features.GoogleIntegration.Models;

public sealed class GoogleUserInfo
{
    [JsonPropertyName("sub")]
    public string Sub { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}
