using FamilyHub.Modules.Auth.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Queries.GetUserFamilies;

/// <summary>
/// Handler for GetUserFamiliesQuery.
/// Retrieves all families that a user belongs to.
/// </summary>
public sealed class GetUserFamiliesQueryHandler(
    IFamilyRepository familyRepository,
    ILogger<GetUserFamiliesQueryHandler> logger)
    : IRequestHandler<GetUserFamiliesQuery, GetUserFamiliesResult>
{
    private readonly IFamilyRepository _familyRepository = familyRepository ?? throw new ArgumentNullException(nameof(familyRepository));
    private readonly ILogger<GetUserFamiliesQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<GetUserFamiliesResult> Handle(
        GetUserFamiliesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Retrieving families for user {UserId}",
            request.UserId.Value);

        // 1. Query families from repository
        var families = await _familyRepository.GetFamiliesByUserIdAsync(request.UserId, cancellationToken);

        _logger.LogInformation(
            "Found {FamilyCount} family(ies) for user {UserId}",
            families.Count,
            request.UserId.Value);

        // 2. Map to DTOs
        var familyDtos = families.Select(f => new FamilyDto
        {
            FamilyId = f.Id,
            Name = f.Name,
            OwnerId = f.OwnerId,
            CreatedAt = f.CreatedAt,
            MemberCount = f.UserFamilies.Count(uf => uf.IsActive)
        }).ToList();

        // 3. Return result
        return new GetUserFamiliesResult
        {
            Families = familyDtos
        };
    }
}
