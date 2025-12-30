using FamilyHub.Modules.Auth.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Queries.GetUserFamilies;

/// <summary>
/// Handler for GetUserFamiliesQuery.
/// Retrieves all families that a user belongs to.
/// </summary>
public sealed partial class GetUserFamiliesQueryHandler(
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
        LogRetrievingFamiliesForUser(request.UserId.Value);

        // 1. Query families from repository
        var families = await _familyRepository.GetFamiliesByUserIdAsync(request.UserId, cancellationToken);

        LogFoundFamilycountFamilyIesForUserUserid(families.Count, request.UserId.Value);

        // 2. Map to DTOs
        var familyDtos = families.Select(f => new FamilyDto
        {
            FamilyId = f.Id,
            Name = f.Name.Value,
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

    [LoggerMessage(LogLevel.Information, "Retrieving families for user {userId}")]
    partial void LogRetrievingFamiliesForUser(Guid userId);

    [LoggerMessage(LogLevel.Information, "Found {familyCount} family(ies) for user {userId}")]
    partial void LogFoundFamilycountFamilyIesForUserUserid(int familyCount, Guid userId);
}
