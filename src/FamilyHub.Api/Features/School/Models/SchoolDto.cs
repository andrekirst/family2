namespace FamilyHub.Api.Features.School.Models;

public class SchoolDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid FamilyId { get; set; }
    public Guid FederalStateId { get; set; }
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
