using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Family.Application.Commands;
using FamilyHub.Api.Features.Family.Domain.Repositories;

namespace FamilyHub.Api.Features.Family.Application.Handlers;

/// <summary>
/// Handler for AddFamilyMemberCommand.
/// Adds a user to an existing family.
/// </summary>
public static class AddFamilyMemberCommandHandler
{
    public static async Task<bool> Handle(
        AddFamilyMemberCommand command,
        IFamilyRepository familyRepository,
        IUserRepository userRepository,
        CancellationToken ct)
    {
        // Get family with members
        var family = await familyRepository.GetByIdWithMembersAsync(command.FamilyId, ct);
        if (family is null)
        {
            return false;
        }

        // Get user to add
        var user = await userRepository.GetByIdAsync(command.UserIdToAdd, ct);
        if (user is null)
        {
            return false;
        }

        // Add member using aggregate method (raises events)
        family.AddMember(user);

        // User.AssignToFamily is called within Family.AddMember

        await familyRepository.SaveChangesAsync(ct);

        return true;
    }
}
