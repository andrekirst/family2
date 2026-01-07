using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Commands.CreateFamily;

/// <summary>
/// Handler for CreateFamilyCommand.
/// Creates a new family and establishes owner membership.
/// User context is automatically provided by UserContextEnrichmentBehavior.
/// </summary>
public sealed partial class CreateFamilyCommandHandler(
    IUserContext userContext,
    IFamilyRepository familyRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateFamilyCommandHandler> logger)
    : IRequestHandler<CreateFamilyCommand, CreateFamilyResult>
{
    public async Task<CreateFamilyResult> Handle(
        CreateFamilyCommand request,
        CancellationToken cancellationToken)
    {
        // Get user context (already loaded and validated by UserContextEnrichmentBehavior)
        var userId = userContext.UserId;
        var user = userContext.User;

        LogCreatingFamilyFamilynameForUserUserid(request.Name, userId.Value);

        // 1. Create new family using domain factory method
        var family = Family.Create(request.Name, userId);

        // 2. Update user's FamilyId to point to new family
        user.UpdateFamily(family.Id);

        // 3. Persist to database
        await familyRepository.AddAsync(family, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogSuccessfullyCreatedFamilyFamilyidFamilynameWithOwnerUserid(family.Id.Value, family.Name, userId.Value);

        // 4. Return result
        return new CreateFamilyResult
        {
            FamilyId = family.Id,
            Name = family.Name,
            OwnerId = family.OwnerId,
            CreatedAt = family.CreatedAt
        };
    }

    [LoggerMessage(LogLevel.Information, "Creating family '{familyName}' for user {userId}")]
    partial void LogCreatingFamilyFamilynameForUserUserid(FamilyName familyName, Guid userId);

    [LoggerMessage(LogLevel.Information, "Successfully created family {familyId} '{familyName}' with owner {userId}")]
    partial void LogSuccessfullyCreatedFamilyFamilyidFamilynameWithOwnerUserid(Guid familyId, FamilyName familyName, Guid userId);
}
