namespace FamilyHub.Infrastructure.Email;

using Microsoft.Extensions.Logging;
using RazorLight;

/// <summary>
/// Razor template rendering service for emails.
/// </summary>
public sealed partial class RazorEmailTemplateService : IEmailTemplateService
{
    private readonly IRazorLightEngine _razorEngine;
    private readonly ILogger<RazorEmailTemplateService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RazorEmailTemplateService"/> class.
    /// </summary>
    /// <param name="razorEngine">The Razor template engine.</param>
    /// <param name="logger">The logger instance.</param>
    public RazorEmailTemplateService(
        IRazorLightEngine razorEngine,
        ILogger<RazorEmailTemplateService> logger)
    {
        _razorEngine = razorEngine;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> RenderTemplateAsync<TModel>(
        string templateName,
        TModel model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            LogRenderingTemplate(templateName);

            var templateKey = $"{templateName}.cshtml";
            var result = await _razorEngine.CompileRenderAsync(templateKey, model);

            LogTemplateRendered(templateName);
            return result;
        }
        catch (Exception ex)
        {
            LogTemplateRenderFailed(templateName, ex);
            throw;
        }
    }

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Rendering email template: {TemplateName}")]
    partial void LogRenderingTemplate(string templateName);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Template rendered successfully: {TemplateName}")]
    partial void LogTemplateRendered(string templateName);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to render template: {TemplateName}")]
    partial void LogTemplateRenderFailed(string templateName, Exception exception);
}
