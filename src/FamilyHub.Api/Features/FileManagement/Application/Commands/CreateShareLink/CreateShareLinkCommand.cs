using FamilyHub.Api.Features.FileManagement.Domain.ValueObjects;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.FileManagement.Application.Commands.CreateShareLink;

public sealed record CreateShareLinkCommand(
    ShareResourceType ResourceType,
    Guid ResourceId,
    DateTime? ExpiresAt,
    string? Password,
    int? MaxDownloads
) : ICommand<CreateShareLinkResult>, IRequireFamily
{
    public UserId UserId { get; init; }
    public FamilyId FamilyId { get; init; }
}
