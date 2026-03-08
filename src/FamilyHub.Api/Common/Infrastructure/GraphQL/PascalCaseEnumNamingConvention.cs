using HotChocolate.Types.Descriptors;

namespace FamilyHub.Api.Common.Infrastructure.GraphQL;

/// <summary>
/// Custom naming convention that preserves PascalCase for GraphQL enum values
/// instead of the default SCREAMING_CASE conversion.
/// E.g., <c>PermissionResourceType.File</c> → <c>File</c> (not <c>FILE</c>).
/// </summary>
public sealed class PascalCaseEnumNamingConvention : DefaultNamingConventions
{
    public override string GetEnumValueName(object value)
    {
        return value.ToString()!;
    }
}
