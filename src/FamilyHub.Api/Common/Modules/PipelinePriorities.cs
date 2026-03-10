namespace FamilyHub.Api.Common.Modules;

public static class PipelinePriorities
{
    public const int DomainEventPublishing = 100;
    public const int Streaming = 195;
    public const int Logging = 200;
    public const int UserResolution = 250;
    public const int InputSanitization = 290;
    public const int Validation = 300;
    public const int Idempotency = 350;
    public const int QueryAsNoTracking = 360;
    public const int Transaction = 400;
}
