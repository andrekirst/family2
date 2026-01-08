using FamilyHub.Modules.Family.Application.Abstractions;
using FamilyHub.Modules.Family.Domain.Aggregates;
using FamilyHub.Modules.Family.Domain.Repositories;
using FamilyHub.Modules.Family.Domain.ValueObjects;
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
    IUnitOfWork unitOfWork,
    ILogger<FamilyService> logger) : IFamilyService
{
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

    public async Task<FamilyHub.SharedKernel.Domain.Result> TransferOwnershipAsync(
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

    public async Task<FamilyDto?> GetFamilyByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        LogGettingFamilyByUserId(userId.Value);

        var family = await familyRepository.GetFamilyByUserIdAsync(userId, cancellationToken);
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

    public async Task<int> GetMemberCountAsync(
        FamilyId familyId,
        CancellationToken cancellationToken = default)
    {
        LogGettingMemberCount(familyId.Value);

        var count = await familyRepository.GetMemberCountAsync(familyId, cancellationToken);

        LogMemberCountRetrieved(familyId.Value, count);

        return count;
    }

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
