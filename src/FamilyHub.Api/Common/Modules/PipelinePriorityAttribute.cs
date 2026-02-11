namespace FamilyHub.Api.Common.Modules;

[AttributeUsage(AttributeTargets.Class)]
public sealed class PipelinePriorityAttribute(int priority) : Attribute
{
    public int Priority { get; } = priority;
}
