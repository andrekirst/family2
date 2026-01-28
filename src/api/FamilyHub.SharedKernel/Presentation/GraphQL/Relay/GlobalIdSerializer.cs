using System.Text;

namespace FamilyHub.SharedKernel.Presentation.GraphQL.Relay;

/// <summary>
/// Serializes and deserializes Relay-compliant global IDs.
/// Global IDs are base64-encoded strings containing type information and entity ID.
/// </summary>
/// <remarks>
/// <para>
/// Global IDs follow the format: base64("TypeName:guid")
/// For example, a User with ID "550e8400-e29b-41d4-a716-446655440000" becomes:
/// "VXNlcjo1NTBlODQwMC1lMjliLTQxZDQtYTcxNi00NDY2NTU0NDAwMDA="
/// </para>
/// <para>
/// This encoding:
/// <list type="bullet">
/// <item><description>Is opaque to clients (they shouldn't parse it)</description></item>
/// <item><description>Contains type information for polymorphic resolution</description></item>
/// <item><description>Is URL-safe when using base64url variant</description></item>
/// </list>
/// </para>
/// </remarks>
public static class GlobalIdSerializer
{
    private const char Separator = ':';

    /// <summary>
    /// Serializes a type name and GUID into a global ID string.
    /// </summary>
    /// <param name="typeName">The GraphQL type name (e.g., "User", "Family").</param>
    /// <param name="id">The entity's internal GUID.</param>
    /// <returns>A base64-encoded global ID string.</returns>
    /// <exception cref="ArgumentException">Thrown when typeName is null or empty.</exception>
    public static string Serialize(string typeName, Guid id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);

        var raw = $"{typeName}{Separator}{id}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }

    /// <summary>
    /// Deserializes a global ID string into its type name and GUID components.
    /// </summary>
    /// <param name="globalId">The base64-encoded global ID.</param>
    /// <returns>A tuple containing the type name and entity GUID.</returns>
    /// <exception cref="ArgumentException">Thrown when globalId is null, empty, or malformed.</exception>
    /// <exception cref="FormatException">Thrown when the global ID format is invalid.</exception>
    public static (string TypeName, Guid Id) Deserialize(string globalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(globalId);

        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(globalId));
            var separatorIndex = raw.IndexOf(Separator);

            if (separatorIndex < 0)
            {
                throw new FormatException($"Invalid global ID format: missing separator '{Separator}'.");
            }

            var typeName = raw[..separatorIndex];
            var idString = raw[(separatorIndex + 1)..];

            if (string.IsNullOrEmpty(typeName))
            {
                throw new FormatException("Invalid global ID format: type name is empty.");
            }

            if (!Guid.TryParse(idString, out var id))
            {
                throw new FormatException($"Invalid global ID format: '{idString}' is not a valid GUID.");
            }

            return (typeName, id);
        }
        catch (FormatException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FormatException($"Failed to deserialize global ID: {globalId}", ex);
        }
    }

    /// <summary>
    /// Attempts to deserialize a global ID string.
    /// </summary>
    /// <param name="globalId">The base64-encoded global ID.</param>
    /// <param name="result">The deserialized type name and ID if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryDeserialize(string globalId, out (string TypeName, Guid Id) result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(globalId))
        {
            return false;
        }

        try
        {
            result = Deserialize(globalId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extracts only the type name from a global ID without full deserialization.
    /// Useful for routing to the correct resolver before loading the entity.
    /// </summary>
    /// <param name="globalId">The base64-encoded global ID.</param>
    /// <returns>The type name portion of the global ID.</returns>
    /// <exception cref="FormatException">Thrown when the global ID format is invalid.</exception>
    public static string ExtractTypeName(string globalId)
    {
        var (typeName, _) = Deserialize(globalId);
        return typeName;
    }
}
