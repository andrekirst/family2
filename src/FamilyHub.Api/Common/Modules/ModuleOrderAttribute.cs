namespace FamilyHub.Api.Common.Modules;

/// <summary>
/// Specifies the registration order for a module discovered by the source generator.
/// Lower values are registered first. Modules without this attribute default to order 1000.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ModuleOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}
