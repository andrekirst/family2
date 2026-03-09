using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Common.Search;
using FamilyHub.Api.Features.EventChain.Application.EventHandlers;
using FamilyHub.Api.Features.EventChain.Application.Search;
using FamilyHub.Api.Features.EventChain.Infrastructure.Messaging;
using FamilyHub.Api.Features.EventChain.Infrastructure.Repositories;
using FamilyHub.Common.Application;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Infrastructure.Orchestrator;
using FamilyHub.EventChain.Infrastructure.Pipeline;
using FamilyHub.EventChain.Infrastructure.Registry;

namespace FamilyHub.Api.Features.EventChain;

[ModuleOrder(500)]
public sealed class EventChainModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        // Registry and repositories
        services.AddSingleton<IChainRegistry, ChainRegistry>();
        services.AddScoped<IChainDefinitionRepository, ChainDefinitionRepository>();
        services.AddScoped<IChainExecutionRepository, ChainExecutionRepository>();
        services.AddScoped<IChainOrchestrator, ChainOrchestrator>();
        services.AddScoped<IChainExecutionDispatcher, CapChainExecutionDispatcher>();
        services.AddScoped<IDomainEventObserver, ChainTriggerHandler>();

        // CAP subscriber for chain execution messages
        services.AddTransient<ChainCapSubscriber>();

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

        // Search
        services.AddScoped<ISearchProvider, EventChainSearchProvider>();
        services.AddSingleton<ICommandPaletteProvider, EventChainCommandPaletteProvider>();
    }
}
