namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateShareLink;

public sealed record CreateShareLinkResult(bool Success, Guid ShareLinkId, string Token);
