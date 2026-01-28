using FamilyHub.SharedKernel.Application.Abstractions.Authorization;
using FamilyHub.SharedKernel.Application.CQRS;

namespace FamilyHub.Modules.Family.Application.Queries.GetUserFamily;

/// <summary>
/// Query to get the current authenticated user's family.
/// Implements IRequireAuthentication to trigger UserContextEnrichmentBehavior.
/// </summary>
public sealed record GetUserFamilyQuery : IQuery<GetUserFamilyResult?>, IRequireAuthentication;
