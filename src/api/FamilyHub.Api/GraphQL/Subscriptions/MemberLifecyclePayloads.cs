using FamilyHub.SharedKernel.Presentation.GraphQL.Relay;
using HotChocolate.Types;

namespace FamilyHub.Api.GraphQL.Subscriptions;

/// <summary>
/// Payload for member profile change events.
/// </summary>
public sealed record MemberProfileChangedPayload
{
    /// <summary>
    /// Global ID of the member whose profile changed.
    /// </summary>
    public required string MemberId { get; init; }

    /// <summary>
    /// Global ID of the family.
    /// </summary>
    public required string FamilyId { get; init; }

    /// <summary>
    /// Name of the field that changed (e.g., "displayName", "birthday").
    /// </summary>
    public required string FieldName { get; init; }

    /// <summary>
    /// Previous value (null if not previously set).
    /// </summary>
    public string? OldValue { get; init; }

    /// <summary>
    /// New value.
    /// </summary>
    public required string NewValue { get; init; }

    /// <summary>
    /// When the change occurred (UTC).
    /// </summary>
    public required DateTime ChangedAt { get; init; }

    /// <summary>
    /// Global ID of the user who made the change.
    /// </summary>
    public required string ChangedBy { get; init; }
}

/// <summary>
/// Payload for member joined events.
/// </summary>
public sealed record MemberJoinedPayload
{
    /// <summary>
    /// Global ID of the new member.
    /// </summary>
    public required string MemberId { get; init; }

    /// <summary>
    /// Global ID of the family.
    /// </summary>
    public required string FamilyId { get; init; }

    /// <summary>
    /// Display name of the new member.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Role assigned to the new member.
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// When the member joined (UTC).
    /// </summary>
    public required DateTime JoinedAt { get; init; }

    /// <summary>
    /// How the member joined (e.g., "INVITATION", "CREATED_FAMILY").
    /// </summary>
    public required string JoinMethod { get; init; }
}

/// <summary>
/// Payload for member left events.
/// </summary>
public sealed record MemberLeftPayload
{
    /// <summary>
    /// Global ID of the member who left.
    /// </summary>
    public required string MemberId { get; init; }

    /// <summary>
    /// Global ID of the family.
    /// </summary>
    public required string FamilyId { get; init; }

    /// <summary>
    /// Display name of the member who left.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// When the member left (UTC).
    /// </summary>
    public required DateTime LeftAt { get; init; }

    /// <summary>
    /// Reason for leaving (e.g., "VOLUNTARY", "REMOVED", "ACCOUNT_DELETED").
    /// </summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Payload for member role change events.
/// </summary>
public sealed record MemberRoleChangedPayload
{
    /// <summary>
    /// Global ID of the member whose role changed.
    /// </summary>
    public required string MemberId { get; init; }

    /// <summary>
    /// Global ID of the family.
    /// </summary>
    public required string FamilyId { get; init; }

    /// <summary>
    /// Previous role.
    /// </summary>
    public required string OldRole { get; init; }

    /// <summary>
    /// New role.
    /// </summary>
    public required string NewRole { get; init; }

    /// <summary>
    /// When the role changed (UTC).
    /// </summary>
    public required DateTime ChangedAt { get; init; }

    /// <summary>
    /// Global ID of the user who made the change.
    /// </summary>
    public required string ChangedBy { get; init; }
}

/// <summary>
/// GraphQL ObjectType for <see cref="MemberProfileChangedPayload"/>.
/// </summary>
public sealed class MemberProfileChangedPayloadType : ObjectType<MemberProfileChangedPayload>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<MemberProfileChangedPayload> descriptor)
    {
        descriptor.Name("MemberProfileChangedPayload");
        descriptor.Description("Payload for member profile change events.");

        descriptor.Field(p => p.MemberId).Type<NonNullType<IdType>>();
        descriptor.Field(p => p.FamilyId).Type<NonNullType<IdType>>();
        descriptor.Field(p => p.FieldName).Type<NonNullType<StringType>>();
        descriptor.Field(p => p.OldValue).Type<StringType>();
        descriptor.Field(p => p.NewValue).Type<NonNullType<StringType>>();
        descriptor.Field(p => p.ChangedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(p => p.ChangedBy).Type<NonNullType<IdType>>();
    }
}

/// <summary>
/// GraphQL ObjectType for <see cref="MemberJoinedPayload"/>.
/// </summary>
public sealed class MemberJoinedPayloadType : ObjectType<MemberJoinedPayload>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<MemberJoinedPayload> descriptor)
    {
        descriptor.Name("MemberJoinedPayload");
        descriptor.Description("Payload for member joined events.");

        descriptor.Field(p => p.MemberId).Type<NonNullType<IdType>>();
        descriptor.Field(p => p.FamilyId).Type<NonNullType<IdType>>();
        descriptor.Field(p => p.DisplayName).Type<NonNullType<StringType>>();
        descriptor.Field(p => p.Role).Type<NonNullType<StringType>>();
        descriptor.Field(p => p.JoinedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(p => p.JoinMethod).Type<NonNullType<StringType>>();
    }
}

/// <summary>
/// GraphQL ObjectType for <see cref="MemberLeftPayload"/>.
/// </summary>
public sealed class MemberLeftPayloadType : ObjectType<MemberLeftPayload>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<MemberLeftPayload> descriptor)
    {
        descriptor.Name("MemberLeftPayload");
        descriptor.Description("Payload for member left events.");

        descriptor.Field(p => p.MemberId).Type<NonNullType<IdType>>();
        descriptor.Field(p => p.FamilyId).Type<NonNullType<IdType>>();
        descriptor.Field(p => p.DisplayName).Type<NonNullType<StringType>>();
        descriptor.Field(p => p.LeftAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(p => p.Reason).Type<StringType>();
    }
}

/// <summary>
/// GraphQL ObjectType for <see cref="MemberRoleChangedPayload"/>.
/// </summary>
public sealed class MemberRoleChangedPayloadType : ObjectType<MemberRoleChangedPayload>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<MemberRoleChangedPayload> descriptor)
    {
        descriptor.Name("MemberRoleChangedPayload");
        descriptor.Description("Payload for member role change events.");

        descriptor.Field(p => p.MemberId).Type<NonNullType<IdType>>();
        descriptor.Field(p => p.FamilyId).Type<NonNullType<IdType>>();
        descriptor.Field(p => p.OldRole).Type<NonNullType<StringType>>();
        descriptor.Field(p => p.NewRole).Type<NonNullType<StringType>>();
        descriptor.Field(p => p.ChangedAt).Type<NonNullType<DateTimeType>>();
        descriptor.Field(p => p.ChangedBy).Type<NonNullType<IdType>>();
    }
}
