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
        var type = typeof(TMessage);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(string) && p.CanRead && p.CanWrite);

        foreach (var property in properties)
        {
            var value = property.GetValue(message) as string;
            if (value is not null && ContainsHtml(value))
            {
                var sanitized = Sanitizer.Sanitize(value);
                property.SetValue(message, sanitized);
            }
        }
    }

    private static bool ContainsHtml(string value) =>
        value.Contains('<') || value.Contains('>') || value.Contains('&');
}
