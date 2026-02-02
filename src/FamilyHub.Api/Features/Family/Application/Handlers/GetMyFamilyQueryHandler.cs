using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Application.Mappers;
using FamilyHub.Api.Features.Family.Application.Queries;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Models;

namespace FamilyHub.Api.Features.Family.Application.Handlers;

/// <summary>
/// Handler for GetMyFamilyQuery.
/// Retrieves the current user's family.
/// </summary>
public static class GetMyFamilyQueryHandler
{
    public static async Task<FamilyDto?> Handle(
        GetMyFamilyQuery query,
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        CancellationToken ct)
    {
        var user = await userRepository.GetByExternalIdAsync(query.ExternalUserId, ct);
        if (user?.FamilyId == null)
        {
            return null;
        }

        var family = await familyRepository.GetByIdWithMembersAsync(user.FamilyId.Value, ct);
        return family is not null ? FamilyMapper.ToDto(family) : null;
    }
}
