using FamilyHub.Modules.Family.Domain.Repositories;
using MediatR;

namespace FamilyHub.Modules.Family.Application.Queries.GetUserFamilies;

/// <summary>
/// Handler for GetUserFamiliesQuery.
/// Retrieves all families that a user belongs to.
/// </summary>
/// <param name="familyRepository">Repository for family data access.</param>
public sealed class GetUserFamiliesQueryHandler(
    IFamilyRepository familyRepository)
    : IRequestHandler<GetUserFamiliesQuery, GetUserFamiliesResult>
{
    /// <summary>
    /// Handles the GetUserFamiliesQuery by retrieving all families for the specified user.
    /// </summary>
    /// <param name="request">The query containing the user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the list of families the user belongs to.</returns>
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

        // 2. Get member count from repository
        var memberCount = await familyRepository.GetMemberCountAsync(family.Id, cancellationToken);

        // 3. Map to DTO
        var familyDto = new FamilyDto
        {
            FamilyId = family.Id,
            Name = family.Name.Value,
            OwnerId = family.OwnerId,
            CreatedAt = family.CreatedAt,
            MemberCount = memberCount
        };

        // 4. Return result (single family in list for backwards compatibility)
        return new GetUserFamiliesResult
        {
            Families = [familyDto]
        };
    }
}
