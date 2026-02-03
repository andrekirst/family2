namespace FamilyHub.Api.Features.Family.Models;

/// <summary>
/// Data transfer object for Family entity
/// </summary>
public class FamilyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
}
