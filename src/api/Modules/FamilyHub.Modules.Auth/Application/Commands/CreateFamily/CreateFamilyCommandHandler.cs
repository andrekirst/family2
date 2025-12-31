using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using FamilyHub.SharedKernel.Domain.Exceptions;
using FamilyHub.SharedKernel.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Commands.CreateFamily;

/// <summary>
/// Handler for CreateFamilyCommand.
/// Creates a new family and establishes owner membership.
/// Extracts authenticated user ID via ICurrentUserService.
/// </summary>
public sealed partial class CreateFamilyCommandHandler(
    IUserRepository userRepository,
    IFamilyRepository familyRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ILogger<CreateFamilyCommandHandler> logger)
    : IRequestHandler<CreateFamilyCommand, CreateFamilyResult>
{
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly IFamilyRepository _familyRepository = familyRepository ?? throw new ArgumentNullException(nameof(familyRepository));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    private readonly ILogger<CreateFamilyCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<CreateFamilyResult> Handle(
        CreateFamilyCommand request,
        CancellationToken cancellationToken)
    {
        // 0. Validate authentication and extract user ID
        var userId = _currentUserService.GetUserId()
            ?? throw new BusinessException("UNAUTHENTICATED", "You must be authenticated to create a family.");

        LogCreatingFamilyFamilynameForUserUserid(request.Name, userId.Value);

        // 1. Validate user exists
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            LogUserUseridNotFound(userId.Value);
            throw new BusinessException("USER_NOT_FOUND", $"User with ID {userId.Value} not found.");
        }

        // 2. Check if user already belongs to a family (business rule: one family per user)
        var existingFamilies = await _familyRepository.GetFamiliesByUserIdAsync(userId, cancellationToken);
        if (existingFamilies.Count > 0)
        {
            LogUserUseridAlreadyBelongsToFamilycountFamilyIes(userId.Value, existingFamilies.Count);
            throw new BusinessException("FAMILY_ALREADY_EXISTS", "User already belongs to a family. Users can only be members of one family at a time.");
        }

        // 3. Create family using domain factory method
        var family = Family.Create(request.Name, userId);

        // 4. Create owner membership
        var ownerMembership = UserFamily.CreateOwnerMembership(userId, family.Id);

        // 5. Link membership to family (bidirectional relationship)
        family.AddMember(ownerMembership);

        // 6. Persist to database
        await _familyRepository.AddAsync(family, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        LogSuccessfullyCreatedFamilyFamilyidFamilynameWithOwnerUserid(family.Id.Value, family.Name, userId.Value);

        // 7. Return result
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

    [LoggerMessage(LogLevel.Warning, "User {userId} not found")]
    partial void LogUserUseridNotFound(Guid userId);

    [LoggerMessage(LogLevel.Warning, "User {userId} already belongs to {familyCount} family(ies)")]
    partial void LogUserUseridAlreadyBelongsToFamilycountFamilyIes(Guid userId, int familyCount);

    [LoggerMessage(LogLevel.Information, "Successfully created family {familyId} '{familyName}' with owner {userId}")]
    partial void LogSuccessfullyCreatedFamilyFamilyidFamilynameWithOwnerUserid(Guid familyId, FamilyName familyName, Guid userId);
}
