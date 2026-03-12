namespace FamilyHub.Api.Features.School.Models;

public class ClassAssignmentDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid SchoolId { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public Guid SchoolYearId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Guid FamilyId { get; set; }
    public Guid AssignedByUserId { get; set; }
    public DateTime AssignedAt { get; set; }
    public bool IsCurrent { get; set; }
}
