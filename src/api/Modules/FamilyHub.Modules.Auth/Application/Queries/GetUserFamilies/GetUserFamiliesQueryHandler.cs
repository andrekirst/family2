using FamilyHub.Modules.Family.Application.Abstractions;
using FamilyHub.SharedKernel.Application.CQRS;

namespace FamilyHub.Modules.Auth.Application.Queries.GetUserFamilies;

/// <summary>
/// Handler for GetUserFamiliesQuery.
/// Retrieves all families that a user belongs to.
/// </summary>
/// <param name="familyService">Service for family operations via anti-corruption layer.</param>
public sealed class GetUserFamiliesQueryHandler(
    IFamilyService familyService)
    : IQueryHandler<GetUserFamiliesQuery, GetUserFamiliesResult>
{
    /// <inheritdoc />
    public async Task<GetUserFamiliesResult> Handle(
        GetUserFamiliesQuery request,
        CancellationToken cancellationToken)
    {

        // 1. Query family from service (users now have only one family)
        var familyDto = await familyService.GetFamilyByUserIdAsync(request.UserId, cancellationToken);

        if (familyDto == null)
        {
            return new GetUserFamiliesResult
            {
                Families = []
            };
        }

        // 2. Get member count from service
        var memberCount = await familyService.GetMemberCountAsync(familyDto.Id, cancellationToken);

        // 3. Map to query result DTO
        var queryFamilyDto = new FamilyDto
        {
            FamilyId = familyDto.Id,
            Name = familyDto.Name.Value,
            OwnerId = familyDto.OwnerId,
            CreatedAt = familyDto.CreatedAt,
            MemberCount = memberCount
        };

        // 4. Return result (single family in list for backwards compatibility)
        return new GetUserFamiliesResult
        {
            Families = [queryFamilyDto]
        };
    }
}
