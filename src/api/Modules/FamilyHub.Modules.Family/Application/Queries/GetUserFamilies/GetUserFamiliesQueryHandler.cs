using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Family.Domain.Specifications;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Application.CQRS;

namespace FamilyHub.Modules.Family.Application.Queries.GetUserFamilies;

/// <summary>
/// Handler for GetUserFamiliesQuery.
/// Retrieves all families that a user belongs to.
/// </summary>
/// <param name="familyRepository">Repository for family data access.</param>
/// <param name="userLookupService">Service for cross-module user lookups.</param>
public sealed class GetUserFamiliesQueryHandler(
    IFamilyRepository familyRepository,
    IUserLookupService userLookupService)
    : IQueryHandler<GetUserFamiliesQuery, GetUserFamiliesResult>
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

        // 1. Get user's family ID via cross-module service (users now have only one family)
        var familyId = await userLookupService.GetUserFamilyIdAsync(request.UserId, cancellationToken);

        if (familyId == null)
        {
            return new GetUserFamiliesResult
            {
                Families = []
            };
        }

        // 2. Query family using Specification pattern
        var family = await familyRepository.FindOneAsync(
            new FamilyByIdSpecification(familyId.Value),
            cancellationToken);

        if (family == null)
        {
            return new GetUserFamiliesResult
            {
                Families = []
            };
        }

        // 3. Get member count from cross-module service
        var memberCount = await userLookupService.GetFamilyMemberCountAsync(family.Id, cancellationToken);

        // 4. Map to DTO
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
