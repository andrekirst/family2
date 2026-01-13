using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Family.Application.Abstractions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using FamilyHub.SharedKernel.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Commands.CreateFamily;

/// <summary>
/// Handler for CreateFamilyCommand.
/// Creates a new family and establishes owner membership.
/// User context is automatically provided by UserContextEnrichmentBehavior.
/// </summary>
/// <param name="userContext">The current authenticated user context.</param>
/// <param name="familyService">Service for family operations.</param>
/// <param name="unitOfWork">Unit of work for database transactions.</param>
/// <param name="logger">Logger for structured logging.</param>
public sealed partial class CreateFamilyCommandHandler(
    IUserContext userContext,
    IFamilyService familyService,
    IUnitOfWork unitOfWork,
    ILogger<CreateFamilyCommandHandler> logger)
    : IRequestHandler<CreateFamilyCommand, CreateFamilyResult>
{
    /// <inheritdoc />
    public async Task<CreateFamilyResult> Handle(
        CreateFamilyCommand request,
        CancellationToken cancellationToken)
    {
        // Get user context (already loaded and validated by UserContextEnrichmentBehavior)
        var userId = userContext.UserId;
        var user = userContext.User;

        LogCreatingFamilyFamilynameForUserUserid(request.Name, userId.Value);

        // 1. Create new family via service
        var familyResult = await familyService.CreateFamilyAsync(request.Name, userId, cancellationToken);
        if (familyResult.IsFailure)
        {
            throw new InvalidOperationException($"Failed to create family: {familyResult.Error}");
        }

        var familyDto = familyResult.Value;

        // 2. Update user's FamilyId to point to new family
        user.UpdateFamily(familyDto.Id);

        // 3. Persist user changes (family already persisted by service)
        // Shared AuthDbContext in Phase 3 - user changes saved atomically with family
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogSuccessfullyCreatedFamilyFamilyidFamilynameWithOwnerUserid(familyDto.Id.Value, familyDto.Name, userId.Value);

        // 4. Return result
        return new CreateFamilyResult
        {
            FamilyId = familyDto.Id,
            Name = familyDto.Name,
            OwnerId = familyDto.OwnerId,
            CreatedAt = familyDto.CreatedAt
        };
    }

    [LoggerMessage(LogLevel.Information, "Creating family '{familyName}' for user {userId}")]
    partial void LogCreatingFamilyFamilynameForUserUserid(FamilyName familyName, Guid userId);

    [LoggerMessage(LogLevel.Information, "Successfully created family {familyId} '{familyName}' with owner {userId}")]
    partial void LogSuccessfullyCreatedFamilyFamilyidFamilynameWithOwnerUserid(Guid familyId, FamilyName familyName, Guid userId);
}
