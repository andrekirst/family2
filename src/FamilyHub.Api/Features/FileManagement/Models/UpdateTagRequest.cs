namespace FamilyHub.Api.Features.FileManagement.Models;

public class UpdateTagRequest
{
    public required Guid TagId { get; set; }
    public string? Name { get; set; }
    public string? Color { get; set; }
}
