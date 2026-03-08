using FamilyHub.Api.Common.Infrastructure.FamilyScope;
using FamilyHub.Common.Application;
using FamilyHub.Common.Domain.ValueObjects;
using FamilyHub.Api.Features.Messaging.Domain.ValueObjects;

namespace FamilyHub.Api.Features.Messaging.Application.Commands.CreateConversation;

/// <summary>
/// Command to create a new conversation (Direct, Group, or Family).
/// </summary>
public sealed record CreateConversationCommand(
    FamilyId FamilyId,
    UserId CreatedBy,
    ConversationName Name,
    ConversationType Type,
    IReadOnlyList<Guid> MemberIds
) : ICommand<CreateConversationResult>, IFamilyScoped;
