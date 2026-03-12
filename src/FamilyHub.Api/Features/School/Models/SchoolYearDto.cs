namespace FamilyHub.Api.Features.School.Models;

public class SchoolYearDto
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }
    public Guid FederalStateId { get; set; }
    public int StartYear { get; set; }
    public int EndYear { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
