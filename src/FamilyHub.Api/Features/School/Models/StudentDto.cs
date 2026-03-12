namespace FamilyHub.Api.Features.School.Models;

public class StudentDto
{
    public Guid Id { get; set; }
    public Guid FamilyMemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public Guid FamilyId { get; set; }
    public Guid MarkedByUserId { get; set; }
    public DateTime MarkedAt { get; set; }
    public string? CurrentSchoolName { get; set; }
    public string? CurrentClassName { get; set; }
}
