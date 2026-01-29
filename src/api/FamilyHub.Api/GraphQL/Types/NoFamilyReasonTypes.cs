using HotChocolate.Types;

namespace FamilyHub.Api.GraphQL.Types;

/// <summary>
/// Marker interface for NoFamilyReason union members.
/// Enables polymorphic handling in the FamilyOrReason union type.
/// </summary>
public interface INoFamilyReason
{
    /// <summary>
    /// The reason code (e.g., "NOT_CREATED", "INVITE_PENDING", "LEFT_FAMILY").
    /// </summary>
    string Reason { get; }

    /// <summary>
    /// Human-readable message describing why the user has no family.
    /// </summary>
    string Message { get; }
}

/// <summary>
/// User has not created a family and has no pending invites.
/// </summary>
public sealed record NotCreatedReason : INoFamilyReason
{
    /// <inheritdoc />
    public string Reason => "NOT_CREATED";

    /// <inheritdoc />
    public string Message => "You have not created a family yet.";
}

/// <summary>
/// User has pending invitation(s) but hasn't accepted any.
/// </summary>
public sealed record InvitePendingReason : INoFamilyReason
{
    /// <inheritdoc />
    public string Reason => "INVITE_PENDING";

    /// <inheritdoc />
    public string Message => "You have pending family invitations.";

    /// <summary>
    /// Number of pending invitations the user has.
    /// </summary>
    public required int PendingCount { get; init; }
}

/// <summary>
/// User previously had a family but left or was removed.
/// </summary>
public sealed record LeftFamilyReason : INoFamilyReason
{
    /// <inheritdoc />
    public string Reason => "LEFT_FAMILY";

    /// <inheritdoc />
    public string Message => "You left your previous family.";

    /// <summary>
    /// When the user left the family (UTC).
    /// </summary>
    public required DateTime LeftAt { get; init; }
}

/// <summary>
/// GraphQL ObjectType for <see cref="NotCreatedReason"/>.
/// </summary>
public sealed class NotCreatedReasonType : ObjectType<NotCreatedReason>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<NotCreatedReason> descriptor)
    {
        descriptor.Name("NotCreatedReason");
        descriptor.Description("User has not created a family yet.");

        descriptor.Field(r => r.Reason)
            .Type<NonNullType<StringType>>()
            .Description("Reason code: NOT_CREATED");

        descriptor.Field(r => r.Message)
            .Type<NonNullType<StringType>>()
            .Description("Human-readable message.");
    }
}

/// <summary>
/// GraphQL ObjectType for <see cref="InvitePendingReason"/>.
/// </summary>
public sealed class InvitePendingReasonType : ObjectType<InvitePendingReason>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<InvitePendingReason> descriptor)
    {
        descriptor.Name("InvitePendingReason");
        descriptor.Description("User has pending family invitations.");

        descriptor.Field(r => r.Reason)
            .Type<NonNullType<StringType>>()
            .Description("Reason code: INVITE_PENDING");

        descriptor.Field(r => r.Message)
            .Type<NonNullType<StringType>>()
            .Description("Human-readable message.");

        descriptor.Field(r => r.PendingCount)
            .Type<NonNullType<IntType>>()
            .Description("Number of pending invitations.");
    }
}

/// <summary>
/// GraphQL ObjectType for <see cref="LeftFamilyReason"/>.
/// </summary>
public sealed class LeftFamilyReasonType : ObjectType<LeftFamilyReason>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<LeftFamilyReason> descriptor)
    {
        descriptor.Name("LeftFamilyReason");
        descriptor.Description("User previously had a family but left.");

        descriptor.Field(r => r.Reason)
            .Type<NonNullType<StringType>>()
            .Description("Reason code: LEFT_FAMILY");

        descriptor.Field(r => r.Message)
            .Type<NonNullType<StringType>>()
            .Description("Human-readable message.");

        descriptor.Field(r => r.LeftAt)
            .Type<NonNullType<DateTimeType>>()
            .Description("When the user left the family (UTC).");
    }
}
