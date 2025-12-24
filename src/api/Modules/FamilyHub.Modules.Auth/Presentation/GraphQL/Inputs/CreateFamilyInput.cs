using System.ComponentModel.DataAnnotations;

namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Inputs;

/// <summary>
/// GraphQL input for creating a new family.
/// </summary>
public sealed record CreateFamilyInput
{
    /// <summary>
    /// The name of the family (e.g., "Smith Family").
    /// </summary>
    [Required(ErrorMessage = "Family name is required.")]
    [MaxLength(100, ErrorMessage = "Family name cannot exceed 100 characters.")]
    public required string Name { get; init; }
}
