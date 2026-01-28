using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Family.Domain.Specifications;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Application.CQRS;

namespace FamilyHub.Modules.Family.Application.Queries.GetUserFamily;

/// <summary>
/// Handler for GetUserFamilyQuery.
/// Retrieves the current authenticated user's family.
/// </summary>
/// <param name="familyRepository">Repository for family data access.</param>
/// <param name="userLookupService">Service for cross-module user lookups.</param>
/// <param name="userContext">User context populated by UserContextEnrichmentBehavior.</param>
public sealed class GetUserFamilyQueryHandler(
    IFamilyRepository familyRepository,
    IUserLookupService userLookupService,
    IUserContext userContext)
    : IQueryHandler<GetUserFamilyQuery, GetUserFamilyResult?>
{
    /// <summary>
    /// Handles the GetUserFamilyQuery by retrieving the user's family.
    /// </summary>
    /// <param name="request">The query (no parameters, uses IUserContext).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's family or null if they don't have one.</returns>
    public async Task<GetUserFamilyResult?> Handle(
        GetUserFamilyQuery request,
        CancellationToken cancellationToken)
    {
        // IUserContext is populated by UserContextEnrichmentBehavior in the MediatR pipeline
        var userId = userContext.UserId;

        // Get user's family ID via cross-module service
        var familyId = await userLookupService.GetUserFamilyIdAsync(userId, cancellationToken);
        if (familyId == null)
        {
            return null;
        }

        // Query family using Specification pattern
        var family = await familyRepository.FindOneAsync(
            new FamilyByIdSpecification(familyId.Value),
            cancellationToken);

        if (family == null)
        {
            return null;
        }

        // Map to result DTO
        return new GetUserFamilyResult
        {
            FamilyId = family.Id,
            Name = family.Name.Value,
            OwnerId = family.OwnerId,
            CreatedAt = family.CreatedAt,
            UpdatedAt = family.UpdatedAt
        };
    }
}
