using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Application.Commands;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyEntity = FamilyHub.Api.Features.Family.Domain.Entities.Family;

namespace FamilyHub.Api.Features.Family.Application.Handlers;

/// <summary>
/// Handler for CreateFamilyCommand.
/// Creates a new family and assigns the owner as a member.
/// Publishes FamilyCreatedEvent for downstream event chain processing.
/// </summary>
public static class CreateFamilyCommandHandler
{
    public static async Task<CreateFamilyResult> Handle(
        CreateFamilyCommand command,
        IFamilyRepository familyRepository,
        IUserRepository userRepository,
        CancellationToken ct)
    {
        // Validate: user shouldn't already own a family
        var existingFamily = await familyRepository.GetByOwnerIdAsync(command.OwnerId, ct);
        if (existingFamily is not null)
        {
            throw new DomainException("User already owns a family");
        }

        // Get the user to link them to the new family
        var user = await userRepository.GetByIdAsync(command.OwnerId, ct)
            ?? throw new DomainException("User not found");

        // Create family aggregate (raises FamilyCreatedEvent)
        var family = FamilyEntity.Create(command.Name, command.OwnerId);

        // Add family to repository
        await familyRepository.AddAsync(family, ct);

        // Assign user to family (raises UserFamilyAssignedEvent)
        user.AssignToFamily(family.Id);

        // Save all changes - AppDbContext publishes domain events
        await familyRepository.SaveChangesAsync(ct);

        return new CreateFamilyResult(family.Id);
    }
}
