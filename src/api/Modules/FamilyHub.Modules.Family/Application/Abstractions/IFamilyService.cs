using FamilyHub.Modules.Family.Domain.ValueObjects;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using Result = FamilyHub.SharedKernel.Domain.Result;

namespace FamilyHub.Modules.Family.Application.Abstractions;

/// <summary>
/// Application service for Family bounded context operations.
/// Provides anti-corruption layer for cross-module interactions with Family domain.
/// PHASE 3: Serves as abstraction between Auth and Family modules while sharing AuthDbContext.
/// FUTURE (Phase 5+): Will enable clean separation when FamilyDbContext is introduced.
/// </summary>
public interface IFamilyService
{
    /// <summary>
    /// Creates a new family with the specified name and owner.
    /// </summary>
    /// <param name="name">The family name.</param>
    /// <param name="ownerId">The user ID of the family owner.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing FamilyDto on success, or error on failure.</returns>
    Task<FamilyHub.SharedKernel.Domain.Result<FamilyDto>> CreateFamilyAsync(
        FamilyName name,
        UserId ownerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transfers family ownership to a new owner.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="newOwnerId">The new owner's user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<FamilyHub.SharedKernel.Domain.Result> TransferOwnershipAsync(
        FamilyId familyId,
        UserId newOwnerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a family by its ID.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>FamilyDto if found, null otherwise.</returns>
    Task<FamilyDto?> GetFamilyByIdAsync(
        FamilyId familyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a family by user ID (finds the family the user belongs to).
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>FamilyDto if found, null otherwise.</returns>
    Task<FamilyDto?> GetFamilyByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of members in a family.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of family members.</returns>
    Task<int> GetMemberCountAsync(
        FamilyId familyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a family exists.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if family exists, false otherwise.</returns>
    Task<bool> FamilyExistsAsync(
        FamilyId familyId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Data Transfer Object for Family data crossing bounded contexts.
/// Prevents leaking Family aggregate root across module boundaries (DDD best practice).
/// </summary>
public sealed record FamilyDto
{
    /// <summary>
    /// Gets or initializes the family's unique identifier.
    /// </summary>
    public required FamilyId Id { get; init; }

    /// <summary>
    /// Gets or initializes the family's name.
    /// </summary>
    public required FamilyName Name { get; init; }

    /// <summary>
    /// Gets or initializes the owner's user ID.
    /// </summary>
    public required UserId OwnerId { get; init; }

    /// <summary>
    /// Gets or initializes the timestamp when the family was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets or initializes the timestamp when the family was last updated.
    /// </summary>
    public required DateTime UpdatedAt { get; init; }
}
