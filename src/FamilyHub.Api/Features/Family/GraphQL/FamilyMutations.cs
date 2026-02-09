using FamilyHub.Api.Features.Auth.GraphQL;

namespace FamilyHub.Api.Features.Family.GraphQL;

/// <summary>
/// GraphQL mutations for family management operations.
/// Uses Input → Command pattern per ADR-003.
/// Extends AuthMutations (the root mutation type).
/// </summary>
[ExtendObjectType(typeof(AuthMutations))]
public class FamilyMutations
{
}
