namespace FamilyHub.Api.Features.School.Models;

public class UpdateSchoolYearRequest
{
    public required Guid SchoolYearId { get; set; }
    public required Guid FederalStateId { get; set; }
    public required int StartYear { get; set; }
    public required int EndYear { get; set; }
    public required DateOnly StartDate { get; set; }
    public required DateOnly EndDate { get; set; }
}
