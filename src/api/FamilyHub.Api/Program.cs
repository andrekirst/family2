using FamilyHub.Infrastructure.GraphQL.Filters;
using FamilyHub.Infrastructure.GraphQL.Interceptors;
using FamilyHub.Modules.Auth;
using FamilyHub.Modules.Auth.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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

    // Auth Module registration
    builder.Services.AddAuthModule(builder.Configuration);

    // Hot Chocolate GraphQL configuration
    var graphqlBuilder = builder.Services
        .AddGraphQLServer()
        .AddQueryType(d => d.Name("Query"))
        .AddMutationType(d => d.Name("Mutation"))
        .AddAuthorization() // Enable authorization for GraphQL (requires HotChocolate.AspNetCore.Authorization)
        .AddFiltering()
        .AddSorting()
        .AddProjections()
        .AddErrorFilter<GraphQLErrorFilter>() // Centralized exception → GraphQL error mapping
        .AddDiagnosticEventListener<GraphQlLoggingInterceptor>() // GraphQL operation logging
        .ModifyRequestOptions(opt =>
        {
            opt.IncludeExceptionDetails = builder.Environment.IsDevelopment();
        });

    // Module-based GraphQL type extension registration
    // Automatically discovers and registers all [ExtendObjectType] classes from module assemblies
    // Note: Passing null for loggerFactory to avoid ASP0000 warning (BuildServiceProvider in startup)
    // Registration logs can be enabled by configuring ILoggerFactory after app is built if needed
    graphqlBuilder.AddAuthModuleGraphQlTypes();

    // Future modules can be registered here:
    // graphqlBuilder.AddCalendarModuleGraphQLTypes(null);
    // graphqlBuilder.AddTaskModuleGraphQLTypes(null);
    // graphqlBuilder.AddShoppingModuleGraphQLTypes(null);

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

    var app = builder.Build();

    // Middleware pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAngularApp");

    // Authentication and Authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    // GraphQL endpoint
    app.MapGraphQL();

    // Health check endpoint
    app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
       .WithName("HealthCheck");

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

// Make Program class accessible for integration testing
public partial class Program { }
