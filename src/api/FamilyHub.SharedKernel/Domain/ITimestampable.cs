namespace FamilyHub.SharedKernel.Domain;

/// <summary>
/// Marker interface for entities requiring automatic timestamp management.
/// Entities implementing this interface will have UpdatedAt automatically set by
/// TimestampInterceptor on every save operation.
/// </summary>
/// <remarks>
/// <para><strong>Setter Visibility:</strong></para>
/// <para>
/// The setter must be public to satisfy C# interface requirements (C# does not support
/// internal setters in interface definitions). However, actual modification protection
/// is achieved through:
/// </para>
/// <list type="number">
/// <item>Domain constructors preventing external instantiation (protected constructors)</item>
/// <item>Domain methods NOT setting UpdatedAt (interceptor handles it)</item>
/// <item>Assembly boundaries - infrastructure code uses InternalsVisibleTo for controlled access</item>
/// </list>
///
/// <para><strong>Design Trade-off:</strong></para>
/// <para>
/// While the setter is technically public, the combination of protected constructors,
/// domain method discipline, and assembly isolation provides practical encapsulation.
/// This approach avoids reflection overhead and maintains compile-time type safety.
/// </para>
/// </remarks>
public interface ITimestampable
{
    /// <summary>
    /// When the entity was last updated. Automatically set by TimestampInterceptor.
    /// </summary>
    DateTime UpdatedAt { get; set; }
    DateTime CreatedAt { get; set; }
}
