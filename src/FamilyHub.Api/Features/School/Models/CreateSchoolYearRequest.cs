namespace FamilyHub.Api.Features.School.Models;

public class CreateSchoolYearRequest
{
    public required Guid FederalStateId { get; set; }
    public required int StartYear { get; set; }
    public required int EndYear { get; set; }
    public required DateOnly StartDate { get; set; }
    public required DateOnly EndDate { get; set; }
}
