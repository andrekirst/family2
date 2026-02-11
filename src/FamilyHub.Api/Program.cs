using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Common.Email;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.Behaviors;
using FamilyHub.Api.Common.Infrastructure.GraphQL;
using FamilyHub.Api.Common.Infrastructure.Messaging;
using FamilyHub.Api.Common.Middleware;
using FamilyHub.Api.Common.Services;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Features.Auth.Infrastructure.Repositories;
using FamilyHub.Api.Features.Calendar.Domain.Repositories;
using FamilyHub.Api.Features.Calendar.GraphQL;
using FamilyHub.Api.Features.Calendar.Infrastructure.Repositories;
using FamilyHub.Api.Features.Calendar.Infrastructure.Services;
using FamilyHub.Api.Features.Calendar.Models;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.EventChain.Infrastructure.Orchestrator;
using FamilyHub.EventChain.Infrastructure.Pipeline;
using FamilyHub.EventChain.Infrastructure.Registry;
using FamilyHub.Api.Features.EventChain.Infrastructure.Repositories;
using FamilyHub.Api.Features.EventChain.Infrastructure.Scheduler;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.Application.Services;
using FamilyHub.Api.Features.Family.Infrastructure.Repositories;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configure PostgreSQL database with AppDbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=familyhub;Username=familyhub;Password=familyhub";

// Mediator (source-generated, compile-time handler discovery)
builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.Assemblies = [typeof(Program).Assembly];
    options.PipelineBehaviors =
    [
        typeof(DomainEventPublishingBehavior<,>),  // outermost
        typeof(LoggingBehavior<,>),
        typeof(ValidationBehavior<,>),
        typeof(TransactionBehavior<,>),             // innermost before handler
    ];
});

// Infrastructure services
builder.Services.AddScoped<IDomainEventCollector, DomainEventCollector>();
builder.Services.AddScoped<DomainEventInterceptor>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<ICommandBus, MediatorCommandBus>();
builder.Services.AddScoped<IQueryBus, MediatorQueryBus>();

// Configure DbContext with interceptor
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString);
    options.AddInterceptors(sp.GetRequiredService<DomainEventInterceptor>());
});

// Register FluentValidation validators from assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Configure JWT Bearer authentication with Keycloak
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keycloakAuthority = builder.Configuration["Keycloak:Authority"]
            ?? "http://localhost:8080/realms/FamilyHub";
        var keycloakAudience = builder.Configuration["Keycloak:Audience"]
            ?? "account"; // Keycloak default audience

        options.Authority = keycloakAuthority;
        options.Audience = keycloakAudience;
        options.RequireHttpsMetadata = false; // Development only - set to true in production
        options.MapInboundClaims = false; // Use original JWT claim names (sub, email, etc.)

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

    });

builder.Services.AddAuthorization();

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFamilyRepository, FamilyRepository>();
builder.Services.AddScoped<ICalendarEventRepository, CalendarEventRepository>();
builder.Services.AddScoped<IFamilyMemberRepository, FamilyMemberRepository>();
builder.Services.AddScoped<IFamilyInvitationRepository, FamilyInvitationRepository>();

builder.Services.AddFamilyServices();

// Register application services
builder.Services.AddScoped<FamilyAuthorizationService>();

// Configure calendar cleanup background service
builder.Services.Configure<CalendarCleanupOptions>(
    builder.Configuration.GetSection(CalendarCleanupOptions.SectionName));
builder.Services.AddHostedService<CancelledEventCleanupService>();

// Configure email service
builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// Event Chain Engine services
builder.Services.AddSingleton<IChainRegistry, ChainRegistry>();
builder.Services.AddScoped<IChainDefinitionRepository, ChainDefinitionRepository>();
builder.Services.AddScoped<IChainExecutionRepository, ChainExecutionRepository>();
builder.Services.AddScoped<IChainOrchestrator, ChainOrchestrator>();

// Step execution pipeline (middleware order matters: Logging → CircuitBreaker → Retry → Compensation → ActionHandler)
builder.Services.AddSingleton<LoggingMiddleware>();
builder.Services.AddSingleton<CircuitBreakerMiddleware>();
builder.Services.AddSingleton<RetryMiddleware>();
builder.Services.AddScoped<CompensationMiddleware>();
builder.Services.AddScoped<ActionHandlerMiddleware>();
builder.Services.AddScoped<StepPipeline>(sp => new StepPipeline(new IStepMiddleware[]
{
    sp.GetRequiredService<LoggingMiddleware>(),
    sp.GetRequiredService<CircuitBreakerMiddleware>(),
    sp.GetRequiredService<RetryMiddleware>(),
    sp.GetRequiredService<CompensationMiddleware>(),
    sp.GetRequiredService<ActionHandlerMiddleware>()
}));

// Chain scheduler background service
builder.Services.AddHostedService<ChainSchedulerService>();

// Configure CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Configure GraphQL server with Hot Chocolate
builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddErrorFilter<ValidationExceptionErrorFilter>()
    .AddQueryType<RootQuery>()
    .AddMutationType<RootMutation>()
    .AddTypeExtensionsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// PostgreSQL RLS middleware - must come AFTER authentication
app.UseMiddleware<PostgresRlsMiddleware>();

app.MapGraphQL();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();

// Make Program class accessible to integration tests
public partial class Program { }
