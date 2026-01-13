using FamilyHub.Modules.Family.Application.Abstractions;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Family.Domain.Specifications;
using FamilyHub.SharedKernel.Application.Abstractions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Interfaces;
using Microsoft.Extensions.Logging;
using Result = FamilyHub.SharedKernel.Domain.Result;

namespace FamilyHub.Modules.Family.Application.Services;

/// <summary>
/// Implementation of IFamilyService for Family bounded context operations.
/// PHASE 3: Uses shared AuthDbContext via IUnitOfWork abstraction.
/// FUTURE (Phase 5+): Will use FamilyDbContext when separated.
/// </summary>
public sealed partial class FamilyService(
    IFamilyRepository familyRepository,
    IUserLookupService userLookupService,
    IUnitOfWork unitOfWork,
    ILogger<FamilyService> logger) : IFamilyService
{
    /// <summary>
    /// Creates a new family with the specified name and owner.
    /// </summary>
    /// <param name="name">The name of the family.</param>
    /// <param name="ownerId">The ID of the family owner.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing the created family DTO.</returns>
    public async Task<FamilyHub.SharedKernel.Domain.Result<FamilyDto>> CreateFamilyAsync(
        FamilyName name,
        UserId ownerId,
        CancellationToken cancellationToken = default)
    {
        LogCreatingFamily(name.Value, ownerId.Value);

        try
        {
            // Use domain factory method to create aggregate
            var family = Domain.Aggregates.Family.Create(name, ownerId);

            // Persist through repository abstraction
            await familyRepository.AddAsync(family, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            LogFamilyCreated(family.Id.Value, name.Value, ownerId.Value);

            // Return DTO (not aggregate) - prevents leakage across bounded contexts
            return Result.Success(new FamilyDto
            {
                Id = family.Id,
                Name = family.Name,
                OwnerId = family.OwnerId,
                CreatedAt = family.CreatedAt,
                UpdatedAt = family.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            LogFamilyCreationFailed(name.Value, ownerId.Value, ex.Message);
            return Result.Failure<FamilyDto>($"Failed to create family: {ex.Message}");
        }
    }

    /// <summary>
    /// Transfers ownership of a family to a new owner.
    /// </summary>
    /// <param name="familyId">The ID of the family.</param>
    /// <param name="newOwnerId">The ID of the new owner.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public async Task<Result> TransferOwnershipAsync(
        FamilyId familyId,
        UserId newOwnerId,
        CancellationToken cancellationToken = default)
    {
        LogTransferringOwnership(familyId.Value, newOwnerId.Value);

        try
        {
            var family = await familyRepository.GetByIdAsync(familyId, cancellationToken);
            if (family == null)
            {
                LogFamilyNotFound(familyId.Value);
                return Result.Failure("Family not found");
            }

            // Use domain method to transfer ownership
            family.TransferOwnership(newOwnerId);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            LogOwnershipTransferred(familyId.Value, newOwnerId.Value);

            return Result.Success();
        }
        catch (Exception ex)
        {
            LogOwnershipTransferFailed(familyId.Value, newOwnerId.Value, ex.Message);
            return Result.Failure($"Failed to transfer ownership: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves a family by its ID.
    /// </summary>
    /// <param name="familyId">The ID of the family to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The family DTO if found; otherwise, null.</returns>
    public async Task<FamilyDto?> GetFamilyByIdAsync(
        FamilyId familyId,
        CancellationToken cancellationToken = default)
    {
        LogGettingFamilyById(familyId.Value);

        var family = await familyRepository.GetByIdAsync(familyId, cancellationToken);
        if (family == null)
        {
            LogFamilyNotFound(familyId.Value);
            return null;
        }

        LogFamilyFound(familyId.Value);

        return new FamilyDto
        {
            Id = family.Id,
            Name = family.Name,
            OwnerId = family.OwnerId,
            CreatedAt = family.CreatedAt,
            UpdatedAt = family.UpdatedAt
        };
    }

    /// <summary>
    /// Retrieves a family by the user ID of one of its members.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The family DTO if the user is a member of a family; otherwise, null.</returns>
    public async Task<FamilyDto?> GetFamilyByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        LogGettingFamilyByUserId(userId.Value);

        // Use cross-module service to get user's family ID
        var familyId = await userLookupService.GetUserFamilyIdAsync(userId, cancellationToken);
        if (familyId == null)
        {
            LogNoFamilyFoundForUser(userId.Value);
            return null;
        }

        // Use Specification pattern to get family by ID
        var family = await familyRepository.FindOneAsync(
            new FamilyByIdSpecification(familyId.Value),
            cancellationToken);

        if (family == null)
        {
            LogNoFamilyFoundForUser(userId.Value);
            return null;
        }

        LogFamilyFoundForUser(family.Id.Value, userId.Value);

        return new FamilyDto
        {
            Id = family.Id,
            Name = family.Name,
            OwnerId = family.OwnerId,
            CreatedAt = family.CreatedAt,
            UpdatedAt = family.UpdatedAt
        };
    }

    /// <summary>
    /// Gets the number of members in a family.
    /// </summary>
    /// <param name="familyId">The ID of the family.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of members in the family.</returns>
    public async Task<int> GetMemberCountAsync(
        FamilyId familyId,
        CancellationToken cancellationToken = default)
    {
        LogGettingMemberCount(familyId.Value);

        // Use cross-module service for member count (users are in Auth module)
        var count = await userLookupService.GetFamilyMemberCountAsync(familyId, cancellationToken);

        LogMemberCountRetrieved(familyId.Value, count);

        return count;
    }

    /// <summary>
    /// Checks whether a family with the specified ID exists.
    /// </summary>
    /// <param name="familyId">The ID of the family to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>true if the family exists; otherwise, false.</returns>
    public async Task<bool> FamilyExistsAsync(
        FamilyId familyId,
        CancellationToken cancellationToken = default)
    {
        LogCheckingFamilyExistence(familyId.Value);

        var family = await familyRepository.GetByIdAsync(familyId, cancellationToken);
        var exists = family != null;

        LogFamilyExistenceChecked(familyId.Value, exists);

        return exists;
    }

    // Logging methods using LoggerMessage source generator for high performance
    [LoggerMessage(LogLevel.Information, "Creating family '{familyName}' for owner {ownerId}")]
    partial void LogCreatingFamily(string familyName, Guid ownerId);

    [LoggerMessage(LogLevel.Information, "Successfully created family {familyId} '{familyName}' with owner {ownerId}")]
    partial void LogFamilyCreated(Guid familyId, string familyName, Guid ownerId);

    [LoggerMessage(LogLevel.Error, "Failed to create family '{familyName}' for owner {ownerId}: {error}")]
    partial void LogFamilyCreationFailed(string familyName, Guid ownerId, string error);

    [LoggerMessage(LogLevel.Information, "Transferring ownership of family {familyId} to user {newOwnerId}")]
    partial void LogTransferringOwnership(Guid familyId, Guid newOwnerId);

    [LoggerMessage(LogLevel.Information, "Successfully transferred ownership of family {familyId} to user {newOwnerId}")]
    partial void LogOwnershipTransferred(Guid familyId, Guid newOwnerId);

    [LoggerMessage(LogLevel.Error, "Failed to transfer ownership of family {familyId} to user {newOwnerId}: {error}")]
    partial void LogOwnershipTransferFailed(Guid familyId, Guid newOwnerId, string error);

    [LoggerMessage(LogLevel.Information, "Getting family by ID {familyId}")]
    partial void LogGettingFamilyById(Guid familyId);

    [LoggerMessage(LogLevel.Information, "Family {familyId} not found")]
    partial void LogFamilyNotFound(Guid familyId);

    [LoggerMessage(LogLevel.Information, "Family {familyId} found")]
    partial void LogFamilyFound(Guid familyId);

    [LoggerMessage(LogLevel.Information, "Getting family for user {userId}")]
    partial void LogGettingFamilyByUserId(Guid userId);

    [LoggerMessage(LogLevel.Information, "No family found for user {userId}")]
    partial void LogNoFamilyFoundForUser(Guid userId);

    [LoggerMessage(LogLevel.Information, "Found family {familyId} for user {userId}")]
    partial void LogFamilyFoundForUser(Guid familyId, Guid userId);

    [LoggerMessage(LogLevel.Information, "Getting member count for family {familyId}")]
    partial void LogGettingMemberCount(Guid familyId);

    [LoggerMessage(LogLevel.Information, "Family {familyId} has {count} members")]
    partial void LogMemberCountRetrieved(Guid familyId, int count);

    [LoggerMessage(LogLevel.Information, "Checking if family {familyId} exists")]
    partial void LogCheckingFamilyExistence(Guid familyId);

    [LoggerMessage(LogLevel.Information, "Family {familyId} exists: {exists}")]
    partial void LogFamilyExistenceChecked(Guid familyId, bool exists);
}
