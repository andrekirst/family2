using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Common.Infrastructure.Messaging;
using FamilyHub.Api.Common.Middleware;
using FamilyHub.Api.Features.Auth.Domain.Repositories;
using FamilyHub.Api.Features.Auth.GraphQL;
using FamilyHub.Api.Features.Auth.Infrastructure.Repositories;
using FamilyHub.EventChain.Domain.Repositories;
using FamilyHub.Api.Features.EventChain.GraphQL;
using FamilyHub.EventChain.Infrastructure.Orchestrator;
using FamilyHub.EventChain.Infrastructure.Pipeline;
using FamilyHub.EventChain.Infrastructure.Registry;
using FamilyHub.Api.Features.EventChain.Infrastructure.Repositories;
using FamilyHub.Api.Features.EventChain.Infrastructure.Scheduler;
using FamilyHub.Api.Features.Family.Domain.Repositories;
using FamilyHub.Api.Features.Family.GraphQL;
using FamilyHub.Api.Features.Family.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

// Configure PostgreSQL database with AppDbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=familyhub;Username=familyhub;Password=familyhub";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure Wolverine for CQRS with in-process messaging
builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(Program).Assembly);

    // Local queue for in-process messaging (Phase 0-4)
    // Will be upgraded to RabbitMQ in Phase 5+ for microservices
    opts.LocalQueue("default");
});

// Register command and query bus abstractions
builder.Services.AddScoped<FamilyHub.Common.Application.ICommandBus, WolverineCommandBus>();
builder.Services.AddScoped<FamilyHub.Common.Application.IQueryBus, WolverineQueryBus>();

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
    .AddQueryType<AuthQueries>()
    .AddMutationType<AuthMutations>()
    .AddTypeExtension<FamilyQueries>()
    .AddTypeExtension<FamilyMutations>()
    .AddTypeExtension<ChainQueries>()
    .AddTypeExtension<ChainMutations>();

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
