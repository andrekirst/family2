namespace FamilyHub.Api.Features.FileManagement.Application.Commands.AccessShareLink;

public sealed record AccessShareLinkResult(
    bool Success,
    string ResourceType,
    Guid ResourceId,
    Guid FamilyId);
