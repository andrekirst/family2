using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Configuration;
using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.Audit;
using FamilyHub.Api.Common.Infrastructure.Behaviors;
using FamilyHub.Api.Common.Infrastructure.Configuration;
using FamilyHub.Api.Common.Infrastructure.Configuration.Infisical;
using FamilyHub.Api.Common.Infrastructure.GraphQL;
using FamilyHub.Api.Common.Infrastructure.Interceptors;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Common.Infrastructure.Messaging;
using FamilyHub.Api.Common.Middleware;
using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Common.Development;
using FamilyHub.Api.Common.Infrastructure.HealthChecks;
using FamilyHub.Api.Common.Infrastructure.RateLimiting;
using FamilyHub.Api.Common.Infrastructure.Observability;
using FamilyHub.Api.Common.Infrastructure.Resilience;
using FamilyHub.Api.Common.Infrastructure.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IO.Compression;
using DotNetCore.CAP;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Infisical secrets management (loads secrets from vault into IConfiguration)
builder.Configuration.AddInfisical();

// Strongly-typed configuration options
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.SectionName));
builder.Services.Configure<KeycloakOptions>(builder.Configuration.GetSection(KeycloakOptions.SectionName));
builder.Services.Configure<FrontendConfigOptions>(builder.Configuration.GetSection(FrontendConfigOptions.SectionName));
builder.Services.Configure<FamilyHub.Api.Common.Configuration.LocalizationOptions>(
    builder.Configuration.GetSection(FamilyHub.Api.Common.Configuration.LocalizationOptions.SectionName));
builder.Services.Configure<FeatureFlags>(builder.Configuration.GetSection(FeatureFlags.SectionName));

// Configure forwarded headers for reverse proxy (Traefik)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // In development, accept forwarded headers from any source (Docker network).
    // In production, restrict to known proxy networks for security.
    if (builder.Environment.IsDevelopment())
    {
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    }
});

// Configure PostgreSQL database with AppDbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found. Configure it in appsettings.json or environment variables.");

// Mediator (source-generated, compile-time handler discovery)
builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.Assemblies = [typeof(Program).Assembly];
    options.PipelineBehaviors =
    [
        typeof(DomainEventPublishingBehavior<,>),  // outermost (100)
        typeof(LoggingBehavior<,>),                // logs original command (200)
        typeof(UserResolutionBehavior<,>),         // resolves user, populates UserId/FamilyId (250)
        typeof(InputSanitizationBehavior<,>),      // strips HTML from command strings (290)
        typeof(ValidationBehavior<,>),             // validates enriched command (300)
        typeof(IdempotencyBehavior<,>),            // deduplicates retried commands (350)
        typeof(QueryAsNoTrackingBehavior<,>),      // EF Core no-tracking (360)
        typeof(TransactionBehavior<,>),            // innermost before handler (400)
    ];
});

// TimeProvider for testable time-dependent behavior (.NET 8+ built-in)
builder.Services.AddSingleton(TimeProvider.System);

// Infrastructure services
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, FamilyHub.Api.Common.Infrastructure.Auth.CurrentUserContext>();
builder.Services.AddScoped<IDomainEventCollector, DomainEventCollector>();
builder.Services.AddScoped<DomainEventInterceptor>();
builder.Services.AddSingleton<SlowQueryInterceptor>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<ICommandBus, MediatorCommandBus>();
builder.Services.AddScoped<IQueryBus, MediatorQueryBus>();
builder.Services.AddScoped<IAuditEventPersister, AuditEventPersister>();
builder.Services.AddScoped<IDomainEventObserver, AuditEventHandler>();

// Configure DbContext with interceptor
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.CommandTimeout(30);
        npgsql.EnableRetryOnFailure(maxRetryCount: 3);
        npgsql.MaxBatchSize(42);
        npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
    options.UseSnakeCaseNamingConvention();
    options.AddInterceptors(
        sp.GetRequiredService<DomainEventInterceptor>(),
        sp.GetRequiredService<SlowQueryInterceptor>());
    options.ConfigureWarnings(w =>
        w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// CAP — transactional outbox + RabbitMQ message broker
// Domain events are persisted atomically with business data via PostgreSQL outbox tables.
// CAP auto-creates cap.published and cap.received tables for reliable at-least-once delivery.
builder.Services.AddCap(x =>
{
    x.UsePostgreSql(connectionString);
    x.UseRabbitMQ(rabbit =>
    {
        rabbit.HostName = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
        rabbit.UserName = builder.Configuration["RabbitMQ:UserName"] ?? "familyhub";
        rabbit.Password = builder.Configuration["RabbitMQ:Password"] ?? "familyhub";
        rabbit.Port = int.TryParse(builder.Configuration["RabbitMQ:Port"], out var port) ? port : 5672;
    });
    x.FailedRetryCount = 3;
    x.FailedRetryInterval = 60; // seconds between retries
});

// Hangfire — background job processing with PostgreSQL storage
// Used for: dead-letter reprocessing, scheduled tasks, recurring maintenance, report generation
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(options =>
        options.UseNpgsqlConnection(connectionString)));
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount;
});

// Response Compression — Brotli (primary) + gzip (fallback) for GraphQL responses
// EnableForHttps=false mitigates BREACH attack (compression oracle on HTTPS responses).
// In production behind a reverse proxy, compression is handled at the proxy layer.
builder.Services.AddResponseCompression(opts =>
{
    opts.EnableForHttps = false;
    opts.Providers.Add<BrotliCompressionProvider>();
    opts.Providers.Add<GzipCompressionProvider>();
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/graphql-response+json"]);
});
builder.Services.Configure<BrotliCompressionProviderOptions>(opts => opts.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(opts => opts.Level = CompressionLevel.SmallestSize);

// Register FluentValidation validators from assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register validator groups (Input, Auth, Business marker interfaces)
builder.Services.AddValidatorGroups();

// Localization (.resx-based IStringLocalizer for backend error messages)
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Configure JWT Bearer authentication with Keycloak
var keycloakOptions = builder.Configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Authority = internal URL for OIDC discovery (e.g. http://keycloak:8080/realms/...)
        // Issuer = public URL that appears in JWT "iss" claim (e.g. https://kc-{env}.localhost:4443/realms/...)
        // When running behind a reverse proxy, these differ. If Issuer is not set, it defaults to Authority.
        // Support multiple issuers for dual-domain (*.localhost + *.dev.andrekirst.de)
        var keycloakIssuers = keycloakOptions.Issuers?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        options.Authority = keycloakOptions.Authority;
        options.Audience = keycloakOptions.Audience;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.MapInboundClaims = false; // Use original JWT claim names (sub, email, etc.)

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        if (keycloakIssuers is { Length: > 0 })
        {
            options.TokenValidationParameters.ValidIssuers = keycloakIssuers;
        }
        else
        {
            options.TokenValidationParameters.ValidIssuer =
                string.IsNullOrEmpty(keycloakOptions.Issuer) ? keycloakOptions.Authority : keycloakOptions.Issuer;
        }

    });

builder.Services.AddAuthorization();

// Exception handlers (IExceptionHandler pipeline)
builder.Services.AddExceptionHandler<FamilyHub.Api.Features.GoogleIntegration.Infrastructure.GoogleOAuthExceptionHandler>();
builder.Services.AddProblemDetails();

// Controllers removed — all REST endpoints use Minimal API via IEndpointModule

// Feature Modules (auto-discovered via source generator, ordered by [ModuleOrder] attribute)
builder.Services.RegisterAllModules(builder.Configuration);

// Development-only: seed database with Keycloak test users
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<DevDataSeeder>();
}

// Configure CORS for Angular frontend (supports multi-environment via config)
var corsOrigins = builder.Configuration["CORS:Origins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? ["http://localhost:4200"];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
            .WithHeaders("Authorization", "Content-Type", "X-Idempotency-Key", "Apollo-Require-Preflight")
            .WithMethods("GET", "POST", "OPTIONS")
            .AllowCredentials();
    });
});

// Health checks — liveness (self) + readiness (PostgreSQL, Keycloak, JWT, GraphQL)
builder.Services.AddApplicationHealthChecks(builder.Configuration);

// Polly resilience pipelines (database retry, HTTP client retry/circuit-breaker/timeout)
builder.Services.AddResiliencePipelines();

// Per-endpoint rate limiting (graphql-query, graphql-mutation, email-sending, global)
builder.Services.AddApplicationRateLimiting();

// OpenTelemetry — traces + metrics with OTLP export
builder.Services.AddObservability(builder.Configuration);

// Configure GraphQL server with Hot Chocolate
builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddErrorFilter<ValidationExceptionErrorFilter>()
    .AddErrorFilter<BusinessLogicExceptionErrorFilter>()
    .AddQueryType<RootQuery>()
    .AddMutationType<RootMutation>()
    .AddSubscriptionType(d => d.Name("Subscription"))
    .AddInMemorySubscriptions()
    .AddTypeExtensionsFromAssembly(typeof(Program).Assembly)
    .ModifyPagingOptions(opt =>
    {
        opt.DefaultPageSize = 25;
        opt.MaxPageSize = 100;
        opt.IncludeTotalCount = true;
    })
    .AddMaxExecutionDepthRule(15)
    .AllowIntrospection(builder.Environment.IsDevelopment())
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = builder.Environment.IsDevelopment())
    .InitializeOnStartup();

var app = builder.Build();

// Pre-warm OIDC discovery — fetch Keycloak's signing keys at startup so the
// first authenticated request doesn't pay the discovery round-trip cost.
_ = Task.Run(async () =>
{
    try
    {
        var jwtOptions = app.Services
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);
        if (jwtOptions.ConfigurationManager is not null)
        {
            await jwtOptions.ConfigurationManager.GetConfigurationAsync(CancellationToken.None);
            app.Logger.LogInformation("OIDC discovery pre-warmed successfully");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "OIDC discovery pre-warm failed — first request will trigger lazy discovery");
    }
});

// Forward proxy headers (must be first middleware to set correct scheme/host)
app.UseForwardedHeaders();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Auto-apply database migrations in development (supports Docker environments)
    var migrationResult = DatabaseMigrationRunner.Migrate(connectionString, app.Logger);
    if (!migrationResult.Successful)
    {
        throw migrationResult.Error;
    }
}

// Security headers (OWASP baseline)
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'";
    }
    await next();
});

app.UseResponseCompression();
app.UseCors();
app.UseExceptionHandler();

// Request localization — sets CultureInfo.CurrentUICulture from Accept-Language header.
// Must come before authentication so IStringLocalizer resolves the correct locale per-request.
app.UseRequestLocalization(options =>
{
    var localizationConfig = app.Services.GetRequiredService<IOptions<FamilyHub.Api.Common.Configuration.LocalizationOptions>>().Value;
    options.SetDefaultCulture(localizationConfig.DefaultLocale);
    options.AddSupportedCultures(localizationConfig.SupportedLocales);
    options.AddSupportedUICultures(localizationConfig.SupportedLocales);
    options.ApplyCurrentCultureToResponseHeaders = true;
});

app.UseAuthentication();
app.UseAuthorization();

// Rate limiting — must come after authentication so user-based partitioning works
app.UseRateLimiter();

// Locale resolution: DB preference > Accept-Language > "en"
app.UseMiddleware<RequestLocaleResolutionMiddleware>();

// PostgreSQL RLS middleware - must come AFTER authentication
// Skipped in Testing environment where InMemoryDatabase doesn't support raw SQL
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseMiddleware<PostgresRlsMiddleware>();
}

app.UseWebSockets();
app.MapGraphQL(); // Banana Cake Pop UI available at /graphql (built into Hot Chocolate)

// Hangfire dashboard (development only, behind authentication in production)
if (app.Environment.IsDevelopment())
{
    app.MapHangfireDashboard("/hangfire").RequireAuthorization();
}

// Minimal API endpoints — avatar (common) + module endpoints (IEndpointModule)
app.MapGet("/api/avatars/{avatarId:guid}/{size}",
    FamilyHub.Api.Common.Infrastructure.Avatar.AvatarEndpoints.GetAvatar);
app.MapModuleEndpoints();

// Health check endpoints — liveness, readiness, and combined probes
app.MapHealthCheckEndpoints();

// Frontend runtime configuration endpoint (served same-origin via Traefik proxy)
app.MapGet("/config", (IOptions<FrontendConfigOptions> options) =>
{
    var config = options.Value;
    return Results.Ok(new
    {
        apiUrl = config.ApiUrl,
        keycloak = new
        {
            issuer = config.KeycloakIssuer,
            clientId = config.KeycloakClientId,
            redirectUri = $"{config.AppUrl}/callback",
            postLogoutRedirectUri = config.AppUrl,
            scope = "openid profile email"
        }
    });
}).AllowAnonymous();

// Export GraphQL schema to SDL file (development only, useful for docs and codegen)
if (app.Environment.IsDevelopment())
{
    await app.ExportGraphQLSchemaAsync();
}

await app.RunAsync();

// Make Program class accessible to integration tests
public partial class Program { }
