namespace FamilyHub.Api.Features.FileManagement.Models;

public sealed record RuleActionDto(
    Guid? DestinationFolderId,
    List<Guid>? TagIds);
