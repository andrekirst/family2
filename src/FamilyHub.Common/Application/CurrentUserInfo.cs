using FamilyHub.Common.Domain.ValueObjects;

namespace FamilyHub.Common.Application;

/// <summary>
/// Lightweight DTO representing the current authenticated user's context.
/// Populated by the infrastructure layer (CurrentUserContext) and available
/// to handlers that inject <see cref="ICurrentUserContext"/>.
/// </summary>
public sealed record CurrentUserInfo(
    UserId UserId,
    ExternalUserId ExternalUserId,
    FamilyId? FamilyId);
