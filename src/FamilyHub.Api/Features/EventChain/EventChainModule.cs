using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Features.EventChain.Application.EventHandlers;
using FamilyHub.Api.Features.EventChain.Infrastructure.Repositories;
using FamilyHub.Api.Features.EventChain.Infrastructure.Scheduler;
using FamilyHub.Common.Application;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Infrastructure.Orchestrator;
using FamilyHub.EventChain.Infrastructure.Pipeline;
using FamilyHub.EventChain.Infrastructure.Registry;

namespace FamilyHub.Api.Features.EventChain;

public sealed class EventChainModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        // Registry and repositories
        services.AddSingleton<IChainRegistry, ChainRegistry>();
        services.AddScoped<IChainDefinitionRepository, ChainDefinitionRepository>();
        services.AddScoped<IChainExecutionRepository, ChainExecutionRepository>();
        services.AddScoped<IChainOrchestrator, ChainOrchestrator>();
        services.AddScoped<IDomainEventObserver, ChainTriggerHandler>();

        // Step execution pipeline (middleware order: Logging -> CircuitBreaker -> Retry -> Compensation -> ActionHandler)
        services.AddSingleton<LoggingMiddleware>();
        services.AddSingleton<CircuitBreakerMiddleware>();
        services.AddSingleton<RetryMiddleware>();
        services.AddScoped<CompensationMiddleware>();
        services.AddScoped<ActionHandlerMiddleware>();
        services.AddScoped<StepPipeline>(sp => new StepPipeline(new IStepMiddleware[]
        {
            sp.GetRequiredService<LoggingMiddleware>(),
            sp.GetRequiredService<CircuitBreakerMiddleware>(),
            sp.GetRequiredService<RetryMiddleware>(),
            sp.GetRequiredService<CompensationMiddleware>(),
            sp.GetRequiredService<ActionHandlerMiddleware>()
        }));

        // Chain scheduler background service
        services.AddHostedService<ChainSchedulerService>();
    }
}
