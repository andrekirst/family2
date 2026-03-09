using System.Reflection;
using FamilyHub.Api.Common.Modules;
using FamilyHub.Common.Application;
using Ganss.Xss;
using Mediator;

namespace FamilyHub.Api.Common.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that sanitizes all string properties on command messages
/// to prevent stored XSS attacks. Strips HTML tags from user-submitted content
/// before validation runs.
///
/// Priority 290: before validation (300), so validators see clean data.
/// Only applies to commands (not queries).
/// </summary>
[PipelinePriority(PipelinePriorities.InputSanitization)]
public sealed class InputSanitizationBehavior<TMessage, TResponse>
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private static readonly HtmlSanitizer Sanitizer = new()
    {
        // Strip all HTML tags by default — no tags allowed
        // Override per-field if rich text is needed in the future
    };

    static InputSanitizationBehavior()
    {
        Sanitizer.AllowedTags.Clear();
        Sanitizer.AllowedAttributes.Clear();
        Sanitizer.AllowedSchemes.Clear();
    }

    public ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        // Skip for queries — no user input to sanitize
        if (message is IReadOnlyQuery<TResponse>)
        {
            return next(message, cancellationToken);
        }

        SanitizeStringProperties(message);
        return next(message, cancellationToken);
    }

    private static void SanitizeStringProperties(TMessage message)
    {
        SanitizeObject(message, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    private static void SanitizeObject(object? target, HashSet<object> visited)
    {
        if (target is null)
        {
            return;
        }

        var type = target.GetType();

        // Skip value types (Vogen VOs are structs, primitives, enums)
        if (type.IsValueType)
        {
            return;
        }

        // Prevent infinite recursion on circular references
        if (!visited.Add(target))
        {
            return;
        }

        // Skip strings (handled as properties of the parent)
        if (target is string)
        {
            return;
        }

        // Handle collections (List<T>, arrays, etc.)
        if (target is System.Collections.IEnumerable enumerable and not string)
        {
            foreach (var item in enumerable)
            {
                SanitizeObject(item, visited);
            }
            return;
        }

        // Sanitize string properties and recurse into nested objects
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (!property.CanRead)
            {
                continue;
            }

            if (property.PropertyType == typeof(string) && property.CanWrite)
            {
                var value = property.GetValue(target) as string;
                if (value is not null && ContainsHtml(value))
                {
                    var sanitized = Sanitizer.Sanitize(value);
                    property.SetValue(target, sanitized);
                }
            }
            else if (!property.PropertyType.IsValueType && property.PropertyType != typeof(string))
            {
                // Recurse into nested reference-type objects
                try
                {
                    var nested = property.GetValue(target);
                    SanitizeObject(nested, visited);
                }
                catch (TargetInvocationException)
                {
                    // Skip properties that throw on access (e.g., uninitialized lazy properties)
                }
            }
        }
    }

    private static bool ContainsHtml(string value) =>
        value.Contains('<') || value.Contains('>') || value.Contains('&');
}
