using HotChocolate.Resolvers;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyHub.Infrastructure.GraphQL.Directives;

/// <summary>
/// Field middleware that enforces the @visible directive at runtime.
/// </summary>
/// <remarks>
/// <para>
/// This middleware intercepts field resolution and checks if the current user
/// has permission to see the field based on:
/// <list type="bullet">
/// <item><description>The @visible directive's visibility level</description></item>
/// <item><description>The viewer's relationship to the profile owner (owner, family member, or other)</description></item>
/// </list>
/// </para>
/// <para>
/// If the viewer doesn't have sufficient access, the field returns null instead of the actual value.
/// </para>
/// </remarks>
public sealed class VisibilityFieldMiddleware
{
    private readonly FieldDelegate _next;

    /// <summary>
    /// Creates a new instance of the visibility field middleware.
    /// </summary>
    /// <param name="next">The next field delegate in the pipeline.</param>
    public VisibilityFieldMiddleware(FieldDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    /// <summary>
    /// Invokes the middleware to check field visibility before resolution.
    /// </summary>
    /// <param name="context">The middleware context.</param>
    public async Task InvokeAsync(IMiddlewareContext context)
    {
        // Check if the field has the @visible directive
        var visibleDirective = context.Selection.Field.Directives
            .FirstOrDefault(d => d.Type.Name.Equals("visible", StringComparison.OrdinalIgnoreCase));

        if (visibleDirective is null)
        {
            // No directive, proceed normally
            await _next(context);
            return;
        }

        // Get the directive argument
        var visibilityArg = visibleDirective.GetArgumentValue<FieldVisibility>("to");

        // Get the visibility context from scoped services
        var visibilityContext = context.Services.GetService<IVisibilityContext>();

        if (visibilityContext is null)
        {
            // No visibility context available, proceed normally (fallback)
            await _next(context);
            return;
        }

        // Check if the viewer has access
        var hasAccess = await visibilityContext.CanViewFieldAsync(
            context.Parent<object>(),
            context.Selection.Field.Name,
            visibilityArg,
            context.RequestAborted);

        if (!hasAccess)
        {
            // Return null for fields the viewer can't see
            context.Result = null;
            return;
        }

        // Viewer has access, proceed with resolution
        await _next(context);
    }
}
