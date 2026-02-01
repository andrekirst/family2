using FamilyHub.Api.Common.Database;
using FamilyHub.Api.Common.Middleware;
using FamilyHub.Api.Features.Auth.GraphQL;
using FamilyHub.Api.Features.Auth.Services;
using FamilyHub.Api.Features.Family.GraphQL;
using FamilyHub.Api.Features.Family.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configure PostgreSQL database with AppDbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=familyhub;Username=familyhub;Password=familyhub";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure JWT Bearer authentication with Keycloak
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keycloakAuthority = builder.Configuration["Keycloak:Authority"]
            ?? "http://localhost:8080/realms/FamilyHub";
        var keycloakAudience = builder.Configuration["Keycloak:Audience"]
            ?? "familyhub-web";

        options.Authority = keycloakAuthority;
        options.Audience = keycloakAudience;
        options.RequireHttpsMetadata = false; // Development only - set to true in production

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5),
            RoleClaimType = "family_role" // Custom claim from Keycloak
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"Token validated for user: {context.Principal?.FindFirst("email")?.Value}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Register application services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<FamilyService>();

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
    .AddQueryType(q => q.Name("Query"))
    .AddMutationType(m => m.Name("Mutation"))
    .AddTypeExtension<AuthQueries>()
    .AddTypeExtension<AuthMutations>()
    .AddTypeExtension<FamilyQueries>()
    .AddTypeExtension<FamilyMutations>();

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
