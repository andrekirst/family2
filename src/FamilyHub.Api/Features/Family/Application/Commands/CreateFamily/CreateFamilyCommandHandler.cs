using FamilyHub.Common.Application;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Domain.Entities;
using FamilyHub.Api.Features.Family.Domain.Repositories;
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
        // Get the user to link them to the new family (validator guarantees existence)
        var user = (await userRepository.GetByIdAsync(command.UserId, cancellationToken))!;

        // Create family aggregate (raises FamilyCreatedEvent)
        var family = FamilyEntity.Create(command.Name, command.UserId);

        // Add family to repository
        await familyRepository.AddAsync(family, cancellationToken);

        // Create FamilyMember record with Owner role
        var ownerMember = FamilyMember.Create(family.Id, command.UserId, FamilyRole.Owner);
        await familyMemberRepository.AddAsync(ownerMember, cancellationToken);

        // Assign user to family (raises UserFamilyAssignedEvent)
        user.AssignToFamily(family.Id);

        return new CreateFamilyResult(family.Id);
    }
}
