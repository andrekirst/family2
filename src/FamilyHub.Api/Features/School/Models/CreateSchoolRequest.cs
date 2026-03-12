namespace FamilyHub.Api.Features.School.Models;

public class CreateSchoolRequest
{
    public required string Name { get; set; }
    public required Guid FederalStateId { get; set; }
    public required string City { get; set; }
    public required string PostalCode { get; set; }
}
