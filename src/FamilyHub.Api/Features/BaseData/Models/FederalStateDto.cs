namespace FamilyHub.Api.Features.BaseData.Models;

public class FederalStateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Iso3166Code { get; set; } = string.Empty;
}
