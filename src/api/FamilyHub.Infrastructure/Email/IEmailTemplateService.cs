namespace FamilyHub.Infrastructure.Email;

/// <summary>
/// Interface for rendering email templates.
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Renders a Razor template with the specified model.
    /// </summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    /// <param name="templateName">Name of the template (without .cshtml extension).</param>
    /// <param name="model">The template model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Rendered HTML content.</returns>
    Task<string> RenderTemplateAsync<TModel>(
        string templateName,
        TModel model,
        CancellationToken cancellationToken = default);
}
