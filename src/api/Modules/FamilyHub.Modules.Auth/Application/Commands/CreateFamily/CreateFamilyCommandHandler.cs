using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Domain;
using FamilyHub.Modules.Auth.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyHub.Modules.Auth.Application.Commands.CreateFamily;

/// <summary>
/// Handler for CreateFamilyCommand.
/// Creates a new family and establishes owner membership.
/// </summary>
public sealed class CreateFamilyCommandHandler(
    IUserRepository userRepository,
    IFamilyRepository familyRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateFamilyCommandHandler> logger)
    : IRequestHandler<CreateFamilyCommand, CreateFamilyResult>
{
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly IFamilyRepository _familyRepository = familyRepository ?? throw new ArgumentNullException(nameof(familyRepository));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly ILogger<CreateFamilyCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<CreateFamilyResult> Handle(
        CreateFamilyCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating family '{FamilyName}' for user {UserId}",
            request.Name,
            request.UserId.Value);

        // 1. Validate user exists
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", request.UserId.Value);
            throw new InvalidOperationException($"User with ID {request.UserId.Value} not found.");
        }

        // 2. Check if user already belongs to a family (business rule: one family per user)
        var existingFamilies = await _familyRepository.GetFamiliesByUserIdAsync(request.UserId, cancellationToken);
        if (existingFamilies.Count > 0)
        {
            _logger.LogWarning(
                "User {UserId} already belongs to {FamilyCount} family(ies)",
                request.UserId.Value,
                existingFamilies.Count);
            throw new InvalidOperationException($"User already belongs to a family. Users can only be members of one family at a time.");
        }

        // 3. Create family using domain factory method
        var family = Family.Create(request.Name, request.UserId);

        // 4. Create owner membership
        var ownerMembership = UserFamily.CreateOwnerMembership(request.UserId, family.Id);

        // 5. Link membership to family (bidirectional relationship)
        family.AddMember(ownerMembership);

        // 6. Persist to database
        await _familyRepository.AddAsync(family, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Successfully created family {FamilyId} '{FamilyName}' with owner {UserId}",
            family.Id.Value,
            family.Name,
            request.UserId.Value);

        // 7. Return result
        return new CreateFamilyResult
        {
            FamilyId = family.Id,
            Name = family.Name,
            OwnerId = family.OwnerId,
            CreatedAt = family.CreatedAt
        };
    }
}
