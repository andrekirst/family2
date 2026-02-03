namespace FamilyHub.Api.Features.Auth.Models;

/// <summary>
/// Request model for user registration via OAuth callback
/// </summary>
public class RegisterUserRequest
{
    public required string Email { get; set; }
    public required string Name { get; set; }
    public required string ExternalUserId { get; set; }
    public required string ExternalProvider { get; set; }
    public bool EmailVerified { get; set; }
}
