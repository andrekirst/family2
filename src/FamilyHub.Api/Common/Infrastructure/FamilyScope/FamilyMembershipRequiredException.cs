using FamilyHub.Common.Domain;

namespace FamilyHub.Api.Common.Infrastructure.FamilyScope;

/// <summary>
/// Exception thrown when a user attempts an action that requires family membership
/// but is not a member of any (or the specified) family.
/// </summary>
public sealed class FamilyMembershipRequiredException()
    : DomainException(
        "You must be a member of a family to perform this action.",
        DomainErrorCodes.FamilyMembershipRequired);
