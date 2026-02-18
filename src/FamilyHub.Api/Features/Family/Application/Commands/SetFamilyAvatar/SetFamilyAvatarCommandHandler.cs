using FamilyHub.Common.Application;
using FamilyHub.Common.Domain;
using FamilyHub.Api.Common.Infrastructure.Avatar;
using FamilyHub.Api.Features.Family.Domain.Repositories;

namespace FamilyHub.Api.Features.Family.Application.Commands.SetFamilyAvatar;

/// <summary>
/// Handler for SetFamilyAvatarCommand.
/// Sets a per-family avatar override on the user's FamilyMember entity.
/// </summary>
public sealed class SetFamilyAvatarCommandHandler(
    IFamilyMemberRepository familyMemberRepository,
    IAvatarRepository avatarRepository)
    : ICommandHandler<SetFamilyAvatarCommand, SetFamilyAvatarResult>
{
    public async ValueTask<SetFamilyAvatarResult> Handle(
        SetFamilyAvatarCommand command,
        CancellationToken cancellationToken)
    {
        var member = await familyMemberRepository.GetByUserAndFamilyAsync(
            command.UserId, command.FamilyId, cancellationToken)
            ?? throw new DomainException("Family member not found");

        // Verify the avatar exists
        var avatar = await avatarRepository.GetByIdAsync(command.AvatarId, cancellationToken)
            ?? throw new DomainException("Avatar not found");

        member.SetFamilyAvatar(avatar.Id);

        return new SetFamilyAvatarResult(true);
    }
}
