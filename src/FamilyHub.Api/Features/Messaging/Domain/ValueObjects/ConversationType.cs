namespace FamilyHub.Api.Features.Messaging.Domain.ValueObjects;

/// <summary>
/// The type of conversation determines membership rules and folder organization.
/// Family: auto-created "General" channel for the whole family.
/// Direct: private 1:1 conversation between two members.
/// Group: named conversation with 2+ members.
/// </summary>
public enum ConversationType
{
    Family,
    Direct,
    Group
}
