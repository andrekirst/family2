namespace FamilyHub.Api.Common.Modules;

public static class PipelinePriorities
{
    public const int DomainEventPublishing = 100;
    public const int Logging = 200;
    public const int Validation = 300;
    public const int Transaction = 400;
}
