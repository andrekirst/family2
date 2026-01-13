using AspNetCoreRateLimit;
using FamilyHub.Infrastructure.GraphQL.Filters;
using FamilyHub.Infrastructure.GraphQL.Interceptors;
using FamilyHub.Infrastructure.Messaging;
using FamilyHub.Modules.Auth;
using FamilyHub.Modules.Auth.Application.Abstractions;
using FamilyHub.Modules.Auth.Infrastructure.BackgroundJobs;
using FamilyHub.Modules.Auth.Infrastructure.Configuration;
using FamilyHub.Modules.Family;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Serilog;
using System.Net.Mime;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting Family Hub API");

    // CORS configuration for Angular frontend
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularApp", policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    // Module registrations
    builder.Services.AddAuthModule(builder.Configuration);
    builder.Services.AddFamilyModule(builder.Configuration);

    // RabbitMQ messaging infrastructure
    builder.Services.AddRabbitMq(builder.Configuration);

    // Health checks
    builder.Services.AddHealthChecks()
        .AddRabbitMqHealthCheck("rabbitmq", tags: ["ready", "infrastructure"]);

    // Quartz.NET Background Jobs Configuration
    builder.Services.AddQuartz(q =>
    {
        q.UseSimpleTypeLoader();
        q.UseInMemoryStore();
        q.UseDefaultThreadPool(tp => { tp.MaxConcurrency = 10; });

        // TODO: Job 1: Managed Account Retry Job (runs every 1 minute)
        // Implement ManagedAccountRetryJob class in Infrastructure/BackgroundJobs before enabling
        // var managedAccountRetryJobKey = new JobKey("ManagedAccountRetryJob");
        // q.AddJob<ManagedAccountRetryJob>(opts => opts.WithIdentity(managedAccountRetryJobKey));
        // q.AddTrigger(opts => opts
        //     .ForJob(managedAccountRetryJobKey)
        //     .WithIdentity("ManagedAccountRetryJob-trigger")
        //     .WithSimpleSchedule(x => x
        //         .WithIntervalInMinutes(1)
        //         .RepeatForever())
        //     .StartNow());

        // Job 2: Expired Invitation Cleanup Job (runs daily at 3 AM UTC)
        var expiredInvitationCleanupJobKey = new JobKey("ExpiredInvitationCleanupJob");
        q.AddJob<ExpiredInvitationCleanupJob>(opts => opts.WithIdentity(expiredInvitationCleanupJobKey));
        q.AddTrigger(opts => opts
            .ForJob(expiredInvitationCleanupJobKey)
            .WithIdentity("ExpiredInvitationCleanupJob-trigger")
            .WithCronSchedule("0 0 3 * * ?") // Daily at 3 AM UTC
            .StartNow());
    });

    builder.Services.AddQuartzHostedService(options =>
    {
        options.WaitForJobsToComplete = true;
    });

    // Rate Limiting Configuration
    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
    builder.Services.AddInMemoryRateLimiting();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

    // Hot Chocolate GraphQL configuration
    var graphqlBuilder = builder.Services
        .AddGraphQLServer()
        .AddQueryType(d => d.Name("Query"))
        .AddMutationType(d => d.Name("Mutation"))
        .AddMutationConventions() // Enable mutation conventions for declarative error handling
        .AddAuthorization() // Enable authorization for GraphQL (requires HotChocolate.AspNetCore.Authorization)
        .AddFiltering()
        .AddSorting()
        .AddProjections() // Re-enabled - works correctly when FamilyType is properly registered
        .AddErrorFilter<GraphQlErrorFilter>() // Centralized exception → GraphQL error mapping
        .AddDiagnosticEventListener<GraphQlLoggingInterceptor>() // GraphQL operation logging
        .ModifyRequestOptions(opt =>
        {
            opt.IncludeExceptionDetails = builder.Environment.IsDevelopment();
        });

    // Module-based GraphQL type registration via auto-discovery
    // All types with [ExtendObjectType] attribute are automatically discovered
    // Only namespace container types (AuthType, InvitationsType) require explicit registration
    graphqlBuilder.AddAuthModuleGraphQlTypes();
    graphqlBuilder.AddFamilyModuleGraphQlTypes();

    // Register DataLoaders for N+1 query prevention
    // BatchDataLoaders: 1:1 mapping (e.g., UserId -> User)
    // GroupedDataLoaders: 1:N mapping (e.g., FamilyId -> [User, User, ...])
    graphqlBuilder
        .AddDataLoader<FamilyHub.Modules.Auth.Presentation.GraphQL.DataLoaders.UserBatchDataLoader>()
        .AddDataLoader<FamilyHub.Modules.Auth.Presentation.GraphQL.DataLoaders.UsersByFamilyGroupedDataLoader>()
        .AddDataLoader<FamilyHub.Modules.Family.Presentation.GraphQL.DataLoaders.FamilyBatchDataLoader>()
        .AddDataLoader<FamilyHub.Modules.Family.Presentation.GraphQL.DataLoaders.InvitationsByFamilyGroupedDataLoader>();

    // Future modules can be registered here:
    // graphqlBuilder.AddCalendarModuleGraphQLTypes();
    // graphqlBuilder.AddTaskModuleGraphQLTypes();
    // graphqlBuilder.AddShoppingModuleGraphQLTypes();
    // graphqlBuilder.AddMealPlanningModuleGraphQLTypes();
    // graphqlBuilder.AddHealthModuleGraphQLTypes();
    // graphqlBuilder.AddFinanceModuleGraphQLTypes();
    // graphqlBuilder.AddCommunicationModuleGraphQLTypes();

    // JWT Authentication configuration (Zitadel OAuth)
    var zitadelSettings = builder.Configuration.GetSection(ZitadelSettings.SectionName).Get<ZitadelSettings>()
        ?? throw new InvalidOperationException("Zitadel settings are not configured");

    // TODO Use IValidateOptions to validate settings
    if (!zitadelSettings.IsValid())
    {
        throw new InvalidOperationException("Zitadel settings are incomplete. Please check appsettings.json.");
    }

    // Clear default claim type mappings to use short claim names (sub, email, etc.)
    System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // Zitadel OIDC authority for automatic JWKS discovery
            options.Authority = zitadelSettings.Authority;
            options.Audience = zitadelSettings.Audience;

            // Allow HTTP for development (Zitadel on localhost:8080)
            options.RequireHttpsMetadata = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = zitadelSettings.Authority,
                ValidateAudience = true,
                ValidAudience = zitadelSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5), // Allow 5-minute clock skew
                ValidateIssuerSigningKey = true,
                // Signing keys automatically fetched from Zitadel's JWKS endpoint
                // (/.well-known/openid-configuration → jwks_uri)
                NameClaimType = "sub", // Zitadel's user ID claim
                RoleClaimType = "role" // Role claims
            };

            // Configure JWT Bearer authentication for GraphQL
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Allow JWT from Authorization header or from query string for GraphQL subscriptions
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/graphql"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    Log.Warning("JWT authentication failed: {Error}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var userId = context.Principal?.FindFirst("sub")?.Value;
                    Log.Debug("JWT token validated for user: {UserId}", userId);
                    return Task.CompletedTask;
                }
            };
        });

    // Authorization policies
    builder.Services.AddAuthorization();

    // Test environment authentication bypass for k6 performance tests
    // Allows testing authenticated GraphQL endpoints using X-Test-User-Id header
    if (builder.Environment.IsEnvironment("Test"))
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.RemoveAll<IAuthorizationHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, FamilyHub.Api.Infrastructure.TestAuthorizationHandler>();
        builder.Services.RemoveAll<ICurrentUserService>();
        builder.Services.AddScoped<ICurrentUserService, FamilyHub.Api.Infrastructure.HeaderBasedCurrentUserService>();

        Log.Information("Test environment: Auth bypass enabled via X-Test-User-Id header");
    }

    var app = builder.Build();

    // Middleware pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAngularApp");

    // Rate Limiting Middleware
    app.UseIpRateLimiting();

    // Authentication and Authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAuthModule();

    // Family module middleware - placeholder for future expansion
    app.UseFamilyModule();

    // GraphQL endpoint
    app.MapGraphQL();

    // Health check endpoints
    // Main health endpoint with full status
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = WriteHealthCheckResponse
    });

    // RabbitMQ-specific health endpoint
    app.MapHealthChecks("/health/rabbitmq", new HealthCheckOptions
    {
        Predicate = check => check.Name == "rabbitmq",
        ResponseWriter = WriteHealthCheckResponse
    });

    Log.Information("Family Hub API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Writes health check results as JSON response
static Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = MediaTypeNames.Application.Json;

    var result = new
    {
        status = report.Status.ToString(),
        timestamp = DateTime.UtcNow,
        totalDuration = report.TotalDuration.TotalMilliseconds,
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description,
            duration = e.Value.Duration.TotalMilliseconds,
            data = e.Value.Data,
            exception = e.Value.Exception?.Message
        })
    };

    return context.Response.WriteAsJsonAsync(result);
}

/// <summary>
/// Main application entry point.
/// Partial class declaration for WebApplicationFactory integration testing.
/// </summary>
public partial class Program { }
