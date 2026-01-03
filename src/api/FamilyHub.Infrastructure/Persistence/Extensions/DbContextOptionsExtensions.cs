using FamilyHub.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyHub.Infrastructure.Persistence.Extensions;

/// <summary>
/// Extension methods for configuring DbContext with common interceptors.
/// </summary>
public static class DbContextOptionsExtensions
{
    /// <summary>
    /// Adds TimestampInterceptor to DbContext configuration.
    /// </summary>
    /// <param name="optionsBuilder">The DbContext options builder.</param>
    /// <param name="serviceProvider">The service provider for resolving TimeProvider.</param>
    /// <returns>The options builder for method chaining.</returns>
    /// <remarks>
    /// <para><strong>Requirements:</strong></para>
    /// <para>
    /// TimeProvider must be registered in the DI container before calling this method.
    /// Typically registered as: <c>services.AddSingleton(TimeProvider.System)</c>
    /// </para>
    ///
    /// <para><strong>Usage Example:</strong></para>
    /// <code>
    /// services.AddPooledDbContextFactory&lt;AuthDbContext&gt;((sp, options) =>
    /// {
    ///     options.UseNpgsql(connectionString)
    ///         .UseSnakeCaseNamingConvention()
    ///         .AddTimestampInterceptor(sp);
    /// });
    /// </code>
    ///
    /// <para><strong>Future Modules:</strong></para>
    /// <para>
    /// All future modules (Calendar, Task, Shopping, etc.) should follow this pattern
    /// for consistent timestamp management across the modular monolith.
    /// </para>
    /// </remarks>
    public static DbContextOptionsBuilder AddTimestampInterceptor(
        this DbContextOptionsBuilder optionsBuilder,
        IServiceProvider serviceProvider)
    {
        var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();
        var interceptor = new TimestampInterceptor(timeProvider);

        return optionsBuilder.AddInterceptors(interceptor);
    }
}
