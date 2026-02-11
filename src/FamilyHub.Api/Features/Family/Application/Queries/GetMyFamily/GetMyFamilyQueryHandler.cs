using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Application.Mappers;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Models;

namespace FamilyHub.Api.Features.Family.Application.Queries.GetMyFamily;

/// <summary>
/// Handler for GetMyFamilyQuery.
/// Retrieves the current user's family.
/// </summary>
public sealed class GetMyFamilyQueryHandler(
    IUserRepository userRepository,
    IFamilyRepository familyRepository)
    : IQueryHandler<GetMyFamilyQuery, FamilyDto?>
{
    public async ValueTask<FamilyDto?> Handle(
        GetMyFamilyQuery query,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdAsync(query.ExternalUserId, cancellationToken);
        if (user?.FamilyId == null)
        {
            return null;
        }

        var family = await familyRepository.GetByIdWithMembersAsync(user.FamilyId.Value, cancellationToken);
        return family is not null ? FamilyMapper.ToDto(family) : null;
    }
}
