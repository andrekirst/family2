namespace FamilyHub.Api.Features.Family.Models;

/// <summary>
/// Family entity representing a household unit in the Family Hub
/// </summary>
public class Family
{
    /// <summary>
    /// Unique identifier for the family
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Family name (e.g., "Smith Family")
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// User ID of the family owner (creator)
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// When the family was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the family record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the family owner
    /// </summary>
    public Auth.Models.User Owner { get; set; } = null!;

    /// <summary>
    /// Navigation property to all family members
    /// </summary>
    public ICollection<Auth.Models.User> Members { get; set; } = new List<Auth.Models.User>();
}
