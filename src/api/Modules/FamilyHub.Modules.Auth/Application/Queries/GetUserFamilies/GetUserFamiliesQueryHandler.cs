using FamilyHub.Modules.Auth.Domain.Repositories;
using MediatR;

namespace FamilyHub.Modules.Auth.Application.Queries.GetUserFamilies;

/// <summary>
/// Handler for GetUserFamiliesQuery.
/// Retrieves all families that a user belongs to.
/// </summary>
public sealed class GetUserFamiliesQueryHandler(
    IFamilyRepository familyRepository)
    : IRequestHandler<GetUserFamiliesQuery, GetUserFamiliesResult>
{
    public async Task<GetUserFamiliesResult> Handle(
        GetUserFamiliesQuery request,
        CancellationToken cancellationToken)
    {

        // 1. Query family from repository (users now have only one family)
        var family = await familyRepository.GetFamilyByUserIdAsync(request.UserId, cancellationToken);

        if (family == null)
        {
            return new GetUserFamiliesResult
            {
                Families = []
            };
        }

        // 2. Map to DTO
        var familyDto = new FamilyDto
        {
            FamilyId = family.Id,
            Name = family.Name.Value,
            OwnerId = family.OwnerId,
            CreatedAt = family.CreatedAt,
            MemberCount = family.GetMemberCount()
        };

        // 3. Return result (single family in list for backwards compatibility)
        return new GetUserFamiliesResult
        {
            Families = [familyDto]
        };
    }
}
