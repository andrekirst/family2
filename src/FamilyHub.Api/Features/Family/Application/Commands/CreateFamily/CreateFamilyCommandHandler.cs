using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Family.Domain.ValueObjects;
using FamilyEntity = FamilyHub.Api.Features.Family.Domain.Entities.Family;

namespace FamilyHub.Api.Features.Family.Application.Commands.CreateFamily;

/// <summary>
/// Handler for CreateFamilyCommand.
/// Creates a new family, assigns the owner as a member, and creates a FamilyMember record.
/// Publishes FamilyCreatedEvent for downstream event chain processing.
/// </summary>
public sealed class CreateFamilyCommandHandler(
    IFamilyRepository familyRepository,
    IUserRepository userRepository,
    IFamilyMemberRepository familyMemberRepository)
    : ICommandHandler<CreateFamilyCommand, CreateFamilyResult>
{
    public async ValueTask<CreateFamilyResult> Handle(
        CreateFamilyCommand command,
        CancellationToken cancellationToken)
    {
        // Validate: user shouldn't already own a family
        var existingFamily = await familyRepository.GetByOwnerIdAsync(command.OwnerId, cancellationToken);
        if (existingFamily is not null)
        {
            throw new DomainException("User already owns a family");
        }

        // Get the user to link them to the new family
        var user = await userRepository.GetByIdAsync(command.OwnerId, cancellationToken)
            ?? throw new DomainException("User not found");

        // Create family aggregate (raises FamilyCreatedEvent)
        var family = FamilyEntity.Create(command.Name, command.OwnerId);

        // Add family to repository
        await familyRepository.AddAsync(family, cancellationToken);

        // Create FamilyMember record with Owner role
        var ownerMember = FamilyMember.Create(family.Id, command.OwnerId, FamilyRole.Owner);
        await familyMemberRepository.AddAsync(ownerMember, cancellationToken);

        // Assign user to family (raises UserFamilyAssignedEvent)
        user.AssignToFamily(family.Id);

        return new CreateFamilyResult(family.Id);
    }
}
