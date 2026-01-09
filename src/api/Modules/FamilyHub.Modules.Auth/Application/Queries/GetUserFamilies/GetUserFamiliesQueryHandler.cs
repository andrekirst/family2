using FamilyHub.Modules.Family.Application.Abstractions;
using MediatR;

namespace FamilyHub.Modules.Auth.Application.Queries.GetUserFamilies;

/// <summary>
/// Handler for GetUserFamiliesQuery.
/// Retrieves all families that a user belongs to.
/// </summary>
public sealed class GetUserFamiliesQueryHandler(
    IFamilyService familyService)
    : IRequestHandler<GetUserFamiliesQuery, GetUserFamiliesResult>
{
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
