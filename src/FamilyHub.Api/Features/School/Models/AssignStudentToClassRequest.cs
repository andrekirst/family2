namespace FamilyHub.Api.Features.School.Models;

public class AssignStudentToClassRequest
{
    public required Guid StudentId { get; set; }
    public required Guid SchoolId { get; set; }
    public required Guid SchoolYearId { get; set; }
    public required string ClassName { get; set; }
}
