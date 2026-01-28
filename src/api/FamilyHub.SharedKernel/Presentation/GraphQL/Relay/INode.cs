namespace FamilyHub.SharedKernel.Presentation.GraphQL.Relay;

/// <summary>
/// Marker interface for types that implement the Relay Node specification.
/// Types implementing this interface can be fetched via the root node(id:) query.
/// </summary>
/// <remarks>
/// <para>
/// The Node interface is a core Relay specification that enables:
/// <list type="bullet">
/// <item><description>Global object identification via base64-encoded IDs</description></item>
/// <item><description>Efficient cache normalization in clients like Apollo</description></item>
/// <item><description>Refetching any object by its global ID</description></item>
/// </list>
/// </para>
/// <para>
/// Implementing types must:
/// <list type="number">
/// <item><description>Have a Guid-based identifier</description></item>
/// <item><description>Register a node resolver via <c>.ImplementsNode()</c> in their ObjectType</description></item>
/// <item><description>Return global IDs using <see cref="GlobalIdSerializer"/></description></item>
/// </list>
/// </para>
/// </remarks>
public interface INode
{
    /// <summary>
    /// Gets the entity's internal identifier.
    /// This is the raw GUID, not the global ID exposed to clients.
    /// </summary>
    Guid Id { get; }
}
