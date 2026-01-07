using FamilyHub.Infrastructure.GraphQL.Types;
using FamilyHub.SharedKernel.Domain;

namespace FamilyHub.Infrastructure.GraphQL;

/// <summary>
/// Base class providing shared mapping utilities for all module mappers.
/// Provides common transformation logic for cross-cutting concerns like audit info.
/// </summary>
public static class MapperBase
{
    /// <summary>
    /// Maps audit timestamps to GraphQL AuditInfoType.
    /// Used by all entity mappers to provide consistent audit metadata.
    /// </summary>
    /// <returns>AuditInfoType for GraphQL response</returns>
    public static AuditInfoType AsAuditInfo(this ITimestampable timestampable)
    {
        return new AuditInfoType
        {
            CreatedAt = timestampable.CreatedAt,
            UpdatedAt = timestampable.UpdatedAt
        };
    }
}
