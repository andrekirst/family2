namespace FamilyHub.Api.Features.School.Models;

public class UpdateClassAssignmentRequest
{
    public required Guid ClassAssignmentId { get; set; }
    public required Guid SchoolId { get; set; }
    public required Guid SchoolYearId { get; set; }
    public required string ClassName { get; set; }
}
