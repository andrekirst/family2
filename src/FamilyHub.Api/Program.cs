using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Configuration;
using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.Behaviors;
using FamilyHub.Api.Common.Infrastructure.Configuration.Infisical;
using FamilyHub.Api.Common.Infrastructure.GraphQL;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Common.Infrastructure.Messaging;
using FamilyHub.Api.Common.Middleware;
using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Common.Development;
using FamilyHub.Api.Common.Infrastructure.HealthChecks;
using FamilyHub.Api.Common.Infrastructure.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Infisical secrets management (loads secrets from vault into IConfiguration)
builder.Configuration.AddInfisical();

// Strongly-typed configuration options
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.SectionName));
builder.Services.Configure<KeycloakOptions>(builder.Configuration.GetSection(KeycloakOptions.SectionName));
builder.Services.Configure<FrontendConfigOptions>(builder.Configuration.GetSection(FrontendConfigOptions.SectionName));
builder.Services.Configure<FamilyHub.Api.Common.Configuration.LocalizationOptions>(
    builder.Configuration.GetSection(FamilyHub.Api.Common.Configuration.LocalizationOptions.SectionName));

// Configure forwarded headers for reverse proxy (Traefik)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

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
        typeof(DomainEventPublishingBehavior<,>),  // outermost (100)
        typeof(LoggingBehavior<,>),                // logs original command (200)
        typeof(UserResolutionBehavior<,>),         // resolves user, populates UserId/FamilyId (250)
        typeof(ValidationBehavior<,>),             // validates enriched command (300)
        typeof(QueryAsNoTrackingBehavior<,>),      // EF Core no-tracking (360)
        typeof(TransactionBehavior<,>),            // innermost before handler (400)
    ];
});

// Infrastructure services
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, FamilyHub.Api.Common.Infrastructure.Auth.CurrentUserContext>();
builder.Services.AddScoped<IDomainEventCollector, DomainEventCollector>();
builder.Services.AddScoped<DomainEventInterceptor>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<ICommandBus, MediatorCommandBus>();
builder.Services.AddScoped<IQueryBus, MediatorQueryBus>();

// Configure DbContext with interceptor
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString);
    options.UseSnakeCaseNamingConvention();
    options.AddInterceptors(sp.GetRequiredService<DomainEventInterceptor>());
    options.ConfigureWarnings(w =>
        w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

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
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Health checks for auth infrastructure diagnostics
builder.Services.AddHealthChecks()
    .AddCheck<KeycloakHealthCheck>("keycloak_oidc")
    .AddCheck<JwtSigningKeysHealthCheck>("jwt_signing_keys")
    .AddCheck<GraphQLSchemaHealthCheck>("graphql_schema");

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

// Locale resolution: DB preference > Accept-Language > "en"
app.UseMiddleware<RequestLocaleResolutionMiddleware>();

// PostgreSQL RLS middleware - must come AFTER authentication
// Skipped in Testing environment where InMemoryDatabase doesn't support raw SQL
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseMiddleware<PostgresRlsMiddleware>();
}

app.UseWebSockets();
app.MapGraphQL();

// Minimal API endpoints — avatar (common) + module endpoints (IEndpointModule)
app.MapGet("/api/avatars/{avatarId:guid}/{size}",
    FamilyHub.Api.Common.Infrastructure.Avatar.AvatarEndpoints.GetAvatar);
app.MapModuleEndpoints();

// Health check endpoints
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));
app.MapHealthChecks("/health/auth", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

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

await app.RunAsync();

// Make Program class accessible to integration tests
public partial class Program { }
