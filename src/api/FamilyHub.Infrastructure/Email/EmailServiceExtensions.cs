namespace FamilyHub.Infrastructure.Email;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RazorLight;
using System.Reflection;

/// <summary>
/// Service collection extensions for email infrastructure.
/// </summary>
public static class EmailServiceExtensions
{
    /// <summary>
    /// Registers email services with dependency injection.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>Service collection for chaining.</returns>
    public static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind SMTP settings
        services.Configure<SmtpSettings>(
            configuration.GetSection(SmtpSettings.SectionName));

        // Register RazorLight engine
        var templatePath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Email",
            "Templates");

        services.AddSingleton<IRazorLightEngine>(sp =>
        {
            return new RazorLightEngineBuilder()
                .UseFileSystemProject(templatePath)
                .UseMemoryCachingProvider()
                .Build();
        });

        // Register services
        services.AddScoped<IEmailTemplateService, RazorEmailTemplateService>();
        services.AddSingleton<IEmailService, SmtpEmailService>();
        services.AddSingleton<SmtpHealthCheck>();

        return services;
    }

    /// <summary>
    /// Adds SMTP health check.
    /// </summary>
    public static IHealthChecksBuilder AddSmtpHealthCheck(
        this IHealthChecksBuilder builder,
        string name = "smtp",
        HealthStatus? failureStatus = null,
        params string[] tags)
    {
        return builder.AddCheck<SmtpHealthCheck>(
            name,
            failureStatus,
            tags);
    }
}
