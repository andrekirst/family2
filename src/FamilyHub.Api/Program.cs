using System.Net;
using FamilyHub.Common.Application;
using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Common.Infrastructure;
using FamilyHub.Api.Common.Infrastructure.Behaviors;
using FamilyHub.Api.Common.Infrastructure.GraphQL;
using FamilyHub.Api.Common.Infrastructure.GraphQL.NamespaceTypes;
using FamilyHub.Api.Common.Infrastructure.Messaging;
using FamilyHub.Api.Common.Middleware;
using FamilyHub.Api.Common.Modules;
using FamilyHub.Api.Features.Auth;
using FamilyHub.Api.Features.Calendar;
using FamilyHub.Api.Features.EventChain;
using FamilyHub.Api.Features.Family;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

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
    options.UseSnakeCaseNamingConvention();
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

        // Authority = internal URL for OIDC discovery (e.g. http://keycloak:8080/realms/...)
        // Issuer = public URL that appears in JWT "iss" claim (e.g. https://kc-{env}.localhost:4443/realms/...)
        // When running behind a reverse proxy, these differ. If Issuer is not set, it defaults to Authority.
        var keycloakIssuer = builder.Configuration["Keycloak:Issuer"];

        options.Authority = keycloakAuthority;
        options.Audience = keycloakAudience;
        options.RequireHttpsMetadata = false; // Development only - set to true in production
        options.MapInboundClaims = false; // Use original JWT claim names (sub, email, etc.)

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = string.IsNullOrEmpty(keycloakIssuer) ? keycloakAuthority : keycloakIssuer,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

    });

builder.Services.AddAuthorization();

// Feature Modules (explicit ordering - dependencies flow downward)
builder.Services.RegisterModule<AuthModule>(builder.Configuration);
builder.Services.RegisterModule<FamilyModule>(builder.Configuration);
builder.Services.RegisterModule<CalendarModule>(builder.Configuration);
builder.Services.RegisterModule<EventChainModule>(builder.Configuration);

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

// Configure GraphQL server with Hot Chocolate
builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddErrorFilter<ValidationExceptionErrorFilter>()
    .AddErrorFilter<BusinessLogicExceptionErrorFilter>()
    .AddQueryType<RootQuery>()
    .AddMutationType<RootMutation>()
    .AddTypeExtensionsFromAssembly(typeof(Program).Assembly)
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = builder.Environment.IsDevelopment());

var app = builder.Build();

// Forward proxy headers (must be first middleware to set correct scheme/host)
app.UseForwardedHeaders();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Auto-apply EF Core migrations in development (supports Docker environments)
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// PostgreSQL RLS middleware - must come AFTER authentication
app.UseMiddleware<PostgresRlsMiddleware>();

app.MapGraphQL();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

await app.RunAsync();

// Make Program class accessible to integration tests
public partial class Program { }
