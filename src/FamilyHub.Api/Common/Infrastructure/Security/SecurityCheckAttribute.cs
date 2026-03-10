namespace FamilyHub.Api.Common.Infrastructure.Security;

/// <summary>
/// Marker attribute indicating a security check is performed at this point.
/// Validated by architecture tests to ensure security checks are documented.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class SecurityCheckAttribute(string checkType) : Attribute
{
    public string CheckType { get; } = checkType;
}
