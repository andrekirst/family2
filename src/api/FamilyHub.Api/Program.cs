using System.IdentityModel.Tokens.Jwt;
using System.Net.Mime;
using AspNetCoreRateLimit;
using FamilyHub.Infrastructure.Email;
using FamilyHub.Infrastructure.GraphQL.Directives;
using FamilyHub.Infrastructure.GraphQL.Filters;
using FamilyHub.Infrastructure.GraphQL.Interceptors;
using FamilyHub.Infrastructure.Messaging;
using FamilyHub.Modules.Auth;
using FamilyHub.Modules.Auth.Infrastructure.BackgroundJobs;
using FamilyHub.Modules.Auth.Infrastructure.Configuration;
using FamilyHub.Modules.Auth.Presentation.GraphQL.DataLoaders;
using FamilyHub.Modules.Family;
using FamilyHub.Modules.Family.Presentation.GraphQL.DataLoaders;
using FamilyHub.Modules.UserProfile;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Serilog;

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
    builder.Services.AddUserProfileModule(builder.Configuration);

    // TEST MODE: Enable header-based authentication for E2E tests
    // When enabled, bypasses JWT validation and uses X-Test-User-Id / X-Test-User-Email headers
    // SECURITY: Blocked in Production environment - will throw InvalidOperationException
    var testModeEnabled = builder.Services.TryAddTestMode(
        builder.Configuration,
        builder.Environment.EnvironmentName);

    if (testModeEnabled)
    {
        Log.Warning(
            "Test mode is ENABLED - JWT validation is bypassed. " +
            "Use X-Test-User-Id and X-Test-User-Email headers for authentication.");
    }

    // RabbitMQ messaging infrastructure
    builder.Services.AddRabbitMq(builder.Configuration);

    // Redis (GraphQL subscriptions transport)
    builder.Services.AddRedis(builder.Configuration);

    // Email infrastructure
    builder.Services.AddEmailServices(builder.Configuration);

    // Health checks
    builder.Services.AddHealthChecks()
        .AddRabbitMqHealthCheck(tags: ["ready", "infrastructure"])
        .AddRedisHealthCheck(tags: ["ready", "infrastructure"])
        .AddSmtpHealthCheck(tags: ["ready", "infrastructure"]);

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
        .AddSubscriptionType(d => d.Name("Subscription")) // Enable GraphQL subscriptions
        .AddMutationConventions() // Enable mutation conventions for declarative error handling
        .AddAuthorization() // Enable authorization for GraphQL (requires HotChocolate.AspNetCore.Authorization)
        .TryAddTypeInterceptor<FamilyHub.Infrastructure.GraphQL.Interceptors.AuthorizationTypeInterceptor>() // Apply authorization via IRequireXXX interfaces on mutation classes
        .AddFiltering()
        .AddSorting()
        .AddProjections() // Re-enabled - works correctly when FamilyType is properly registered
        .AddErrorFilter<GraphQlErrorFilter>() // Centralized exception → GraphQL error mapping
        .AddDiagnosticEventListener<GraphQlLoggingInterceptor>() // GraphQL operation logging
        .AddRedisSubscriptions(sp => sp.GetRequiredService<StackExchange.Redis.IConnectionMultiplexer>()) // Redis PubSub for multi-instance subscriptions
        .ModifyRequestOptions(opt =>
        {
            opt.IncludeExceptionDetails = builder.Environment.IsDevelopment();
        });

    // Module-based GraphQL type registration via auto-discovery
    // All types with [ExtendObjectType] attribute are automatically discovered
    // Only namespace container types (AuthType, InvitationsType) require explicit registration
    graphqlBuilder.AddAuthModuleGraphQlTypes();
    graphqlBuilder.AddFamilyModuleGraphQlTypes();
    graphqlBuilder.AddUserProfileModuleGraphQlTypes();

    // Root type extensions for nested namespace structure
    // These provide: query { auth, me, health, node, nodes }
    //                mutation { auth, family }
    graphqlBuilder
        .AddTypeExtension<FamilyHub.Api.GraphQL.RootQueryExtensions>()
        .AddTypeExtension<FamilyHub.Api.GraphQL.RootMutationExtensions>();

    // Me namespace types (consolidated user-centric queries)
    // Provides: query { me { profile, family, pendingInvitations } }
    graphqlBuilder
        .AddType<FamilyHub.Api.GraphQL.Namespaces.MeQueriesType>()
        .AddTypeExtension<FamilyHub.Api.GraphQL.Namespaces.MeQueriesExtensions>();

    // Union type for family state (Family | NoFamilyReason variants)
    graphqlBuilder
        .AddType<FamilyHub.Api.GraphQL.Types.FamilyOrReasonUnionType>()
        .AddType<FamilyHub.Api.GraphQL.Types.NotCreatedReasonType>()
        .AddType<FamilyHub.Api.GraphQL.Types.InvitePendingReasonType>()
        .AddType<FamilyHub.Api.GraphQL.Types.LeftFamilyReasonType>();

    // Family extensions for role-based profile access
    // Extends Family type with: profile(userId: ID!) for cross-member profile queries
    graphqlBuilder
        .AddTypeExtension<FamilyHub.Api.GraphQL.Extensions.MeFamilyExtensions>();

    // Health namespace types (liveness + detailed checks)
    // Provides: query { health { liveness, details } }
    graphqlBuilder
        .AddType<FamilyHub.Api.GraphQL.Namespaces.HealthQueriesType>()
        .AddTypeExtension<FamilyHub.Api.GraphQL.Namespaces.HealthQueriesExtensions>()
        .AddType<FamilyHub.Api.GraphQL.Types.HealthLivenessType>()
        .AddType<FamilyHub.Api.GraphQL.Types.HealthDetailsType>()
        .AddType<FamilyHub.Api.GraphQL.Types.DependencyHealthType>();

    // Member lifecycle subscription types (real-time family updates)
    // Provides: subscription { memberProfileChanged, memberJoined, memberLeft, memberRoleChanged }
    graphqlBuilder
        .AddTypeExtension<FamilyHub.Api.GraphQL.Subscriptions.MemberLifecycleSubscriptions>()
        .AddType<FamilyHub.Api.GraphQL.Subscriptions.MemberProfileChangedPayloadType>()
        .AddType<FamilyHub.Api.GraphQL.Subscriptions.MemberJoinedPayloadType>()
        .AddType<FamilyHub.Api.GraphQL.Subscriptions.MemberLeftPayloadType>()
        .AddType<FamilyHub.Api.GraphQL.Subscriptions.MemberRoleChangedPayloadType>();

    // Cross-module coordinator and visibility services
    // Used by Me namespace to aggregate data from Auth, Family, and UserProfile modules
    builder.Services.AddScoped<FamilyHub.Api.Application.Services.IMeQueryCoordinator,
        FamilyHub.Api.Application.Services.MeQueryCoordinator>();
    builder.Services.AddScoped<FamilyHub.Api.Application.Services.IRoleBasedVisibilityService,
        FamilyHub.Api.Application.Services.RoleBasedVisibilityService>();

    // @visible directive for field-level visibility control
    // Enables: field @visible(to: FAMILY) to restrict field access based on viewer relationship
    builder.Services.AddVisibilityDirectiveServices();
    graphqlBuilder.AddVisibleDirective();

    // Entity-centric subscriptions for real-time updates on Node types
    // Enables: subscription { nodeChanged(nodeId: ID!) { ... } }
    graphqlBuilder.AddTypeExtension<FamilyHub.Infrastructure.GraphQL.Subscriptions.NodeSubscriptions>();
    builder.Services.AddScoped<FamilyHub.SharedKernel.Presentation.GraphQL.Subscriptions.INodeSubscriptionPublisher,
        FamilyHub.Infrastructure.GraphQL.Subscriptions.NodeSubscriptionPublisher>();

    // Register NodeResolver for Relay Node resolution
    builder.Services.AddScoped<FamilyHub.SharedKernel.Presentation.GraphQL.Relay.INodeResolver,
        FamilyHub.SharedKernel.Presentation.GraphQL.Relay.NodeResolver>();

    // Register DataLoaders for N+1 query prevention
    // BatchDataLoaders: 1:1 mapping (e.g., UserId -> User)
    // GroupedDataLoaders: 1:N mapping (e.g., FamilyId -> [User, User, ...])
    graphqlBuilder
        .AddDataLoader<UserBatchDataLoader>()
        .AddDataLoader<UsersByFamilyGroupedDataLoader>()
        .AddDataLoader<FamilyBatchDataLoader>()
        .AddDataLoader<InvitationsByFamilyGroupedDataLoader>();

    // Future modules can be registered here:
    // graphqlBuilder.AddCalendarModuleGraphQLTypes();
    // graphqlBuilder.AddTaskModuleGraphQLTypes();
    // graphqlBuilder.AddShoppingModuleGraphQLTypes();
    // graphqlBuilder.AddMealPlanningModuleGraphQLTypes();
    // graphqlBuilder.AddHealthModuleGraphQLTypes();
    // graphqlBuilder.AddFinanceModuleGraphQLTypes();
    // graphqlBuilder.AddCommunicationModuleGraphQLTypes();

    // JWT Authentication configuration (Local symmetric key)
    var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
        ?? throw new InvalidOperationException("JWT settings are not configured");

    if (!jwtSettings.IsValid())
    {
        throw new InvalidOperationException("JWT settings are incomplete. SecretKey must be at least 32 characters.");
    }

    // Clear default claim type mappings to use short claim names (sub, email, etc.)
    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

    var signingKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // No authority - we use a symmetric signing key
            options.RequireHttpsMetadata = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5), // Allow 5-minute clock skew
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                NameClaimType = "sub", // User ID claim
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

    var app = builder.Build();

    // Middleware pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAngularApp");

    // WebSocket support for GraphQL subscriptions
    app.UseWebSockets();

    // Rate Limiting Middleware
    app.UseIpRateLimiting();

    // Authentication and Authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAuthModule();

    // Family module middleware - placeholder for future expansion
    app.UseFamilyModule();

    // UserProfile module middleware - placeholder for future expansion
    app.UseUserProfileModule();

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

namespace FamilyHub.Api
{
    /// <summary>
    /// Main application entry point.
    /// Partial class declaration for WebApplicationFactory integration testing.
    /// </summary>
    public partial class Program { }
}
