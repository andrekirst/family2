namespace FamilyHub.Api.Features.Auth.Models;

/// <summary>
/// Data transfer object for User entity
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Username { get; set; }
    public Guid? FamilyId { get; set; }
    public bool EmailVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Permissions { get; set; } = [];
}
