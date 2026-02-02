using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Application.Commands;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyEntity = FamilyHub.Api.Features.Family.Domain.Entities.Family;

namespace FamilyHub.Api.Features.Family.Application.Handlers;

/// <summary>
/// Handler for CreateFamilyCommand.
/// Creates a new family and assigns the owner.
/// Wolverine discovers this handler by convention (static Handle method).
/// </summary>
public static class CreateFamilyCommandHandler
{
    public static async Task<CreateFamilyResult> Handle(
        CreateFamilyCommand command,
        IFamilyRepository familyRepository,
        IUserRepository userRepository,
        CancellationToken ct)
    {
        // Verify owner exists
        var owner = await userRepository.GetByIdAsync(command.OwnerId, ct);
        if (owner is null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Check if user already has a family
        if (owner.FamilyId is not null)
        {
            throw new InvalidOperationException("User already belongs to a family");
        }

        // Create family using factory method
        var family = FamilyEntity.Create(command.Name, command.OwnerId);

        await familyRepository.AddAsync(family, ct);

        // Assign owner to family (raises UserFamilyAssignedEvent)
        owner.AssignToFamily(family.Id);

        await familyRepository.SaveChangesAsync(ct);

        return new CreateFamilyResult(family.Id);
    }
}
