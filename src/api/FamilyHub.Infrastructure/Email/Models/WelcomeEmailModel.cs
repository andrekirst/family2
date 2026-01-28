namespace FamilyHub.Infrastructure.Email.Models;

/// <summary>
/// Model for welcome emails after registration.
/// </summary>
public sealed record WelcomeEmailModel
{
    /// <summary>
    /// The user's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// URL to the application.
    /// </summary>
    public required string AppUrl { get; init; }
}