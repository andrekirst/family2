namespace FamilyHub.Api.Features.FileManagement.Models;

public sealed record ShareLinkAccessLogDto(
    Guid Id,
    Guid ShareLinkId,
    string IpAddress,
    string? UserAgent,
    string Action,
    DateTime AccessedAt);
