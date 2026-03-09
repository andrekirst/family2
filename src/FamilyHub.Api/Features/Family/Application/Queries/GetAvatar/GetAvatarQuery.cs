using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetAvatar;

public sealed record GetAvatarQuery(
    Guid AvatarId,
    string Size
) : IReadOnlyQuery<Result<GetAvatarResult>>, IRequireUser
{
    public UserId UserId { get; init; }
}
