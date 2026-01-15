using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FamilyHub.Tests.Integration.Helpers;

/// <summary>
/// MailHog REST API client for backend integration tests.
/// Provides programmatic access to MailHog for email verification during tests.
///
/// MailHog captures emails sent via SMTP (localhost:1025) and exposes them via
/// REST API (localhost:8025).
///
/// Usage:
/// <code>
/// var mailHog = new MailHogClient();
/// await mailHog.ClearEmailsAsync();
///
/// // Trigger email sending in test
/// // ...
///
/// // Verify email received
/// var email = await mailHog.GetEmailByRecipientAsync("user@example.com");
/// email.Should().NotBeNull();
/// email!.Content.Headers["Subject"][0].Should().Contain("invited you");
/// </code>
/// </summary>
public sealed class MailHogClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private const string DefaultBaseUrl = "http://localhost:8025";

    /// <summary>
    /// Initialize MailHog client with default localhost URL
    /// </summary>
    public MailHogClient() : this(DefaultBaseUrl)
    {
    }

    /// <summary>
    /// Initialize MailHog client with custom base URL
    /// </summary>
    /// <param name="baseUrl">Base URL of MailHog API (e.g., http://localhost:8025)</param>
    public MailHogClient(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    /// <summary>
    /// Get all emails from MailHog inbox
    /// </summary>
    /// <returns>List of emails (empty if none found)</returns>
    public async Task<List<MailHogMessage>> GetEmailsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<MailHogResponse>(
                "/api/v2/messages");

            var emails = response?.Items ?? new List<MailHogMessage>();
            Console.WriteLine($"📧 MailHog: Retrieved {emails.Count} emails from API");

            if (emails.Any())
            {
                foreach (var email in emails)
                {
                    var recipients = string.Join(", ", email.To.Select(to => $"{to.Mailbox}@{to.Domain}"));
                    Console.WriteLine($"   - To: {recipients}");
                }
            }

            return emails;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to fetch emails from MailHog: {ex.Message}");
            Console.WriteLine($"   Stack trace: {ex.StackTrace}");
            return new List<MailHogMessage>();
        }
    }

    /// <summary>
    /// Get email by recipient email address
    /// </summary>
    /// <param name="email">Full email address (e.g., "user@example.com")</param>
    /// <returns>Email or null if not found</returns>
    public async Task<MailHogMessage?> GetEmailByRecipientAsync(string email)
    {
        var emails = await GetEmailsAsync();
        return emails.FirstOrDefault(e =>
            e.To.Any(to => $"{to.Mailbox}@{to.Domain}" == email));
    }

    /// <summary>
    /// Get email by sender email address
    /// </summary>
    /// <param name="email">Full email address (e.g., "noreply@familyhub.local")</param>
    /// <returns>Email or null if not found</returns>
    public async Task<MailHogMessage?> GetEmailBySenderAsync(string email)
    {
        var emails = await GetEmailsAsync();
        return emails.FirstOrDefault(e =>
            $"{e.From.Mailbox}@{e.From.Domain}" == email);
    }

    /// <summary>
    /// Get email by subject line (case-insensitive contains)
    /// </summary>
    /// <param name="subject">Subject text to search for</param>
    /// <returns>Email or null if not found</returns>
    public async Task<MailHogMessage?> GetEmailBySubjectAsync(string subject)
    {
        var emails = await GetEmailsAsync();
        return emails.FirstOrDefault(e =>
            e.Content.Headers.TryGetValue("Subject", out var subjects) &&
            subjects.Any(s => s.Contains(subject, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Clear all emails from MailHog inbox
    /// Useful for test cleanup to ensure clean state
    /// </summary>
    public async Task ClearEmailsAsync()
    {
        try
        {
            var response = await _httpClient.DeleteAsync("/api/v1/messages");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    $"Failed to clear MailHog emails: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to clear MailHog emails: {ex.Message}");
        }
    }

    /// <summary>
    /// Wait for an email matching the predicate to arrive
    /// Polls MailHog API at 500ms intervals until email found or timeout
    /// </summary>
    /// <param name="predicate">Function to filter emails</param>
    /// <param name="timeout">Max wait time in milliseconds (default: 5000)</param>
    /// <returns>Email or null if timeout</returns>
    ///
    /// <example>
    /// // Wait for invitation email
    /// var email = await mailHog.WaitForEmailAsync(
    ///     e => e.Content.Body.Contains("invited you"),
    ///     5000
    /// );
    /// </example>
    public async Task<MailHogMessage?> WaitForEmailAsync(
        Func<MailHogMessage, bool> predicate,
        int timeout = 5000)
    {
        var startTime = DateTime.UtcNow;

        while ((DateTime.UtcNow - startTime).TotalMilliseconds < timeout)
        {
            var emails = await GetEmailsAsync();
            var found = emails.FirstOrDefault(predicate);

            if (found != null)
            {
                Console.WriteLine(
                    $"✅ Email found after {(DateTime.UtcNow - startTime).TotalMilliseconds}ms");
                return found;
            }

            // Poll every 500ms
            await Task.Delay(500);
        }

        Console.WriteLine($"⏱️ Timeout: No email found after {timeout}ms");
        return null;
    }

    /// <summary>
    /// Wait for multiple emails matching the predicate
    /// Useful for testing batch operations (e.g., multiple invitations)
    /// </summary>
    /// <param name="count">Expected number of emails</param>
    /// <param name="predicate">Function to filter emails</param>
    /// <param name="timeout">Max wait time in milliseconds (default: 5000)</param>
    /// <returns>Array of emails (may be less than count if timeout)</returns>
    ///
    /// <example>
    /// // Wait for 3 invitation emails
    /// var emails = await mailHog.WaitForEmailsAsync(
    ///     3,
    ///     e => e.Content.Body.Contains("invited you"),
    ///     10000
    /// );
    /// emails.Should().HaveCount(3);
    /// </example>
    public async Task<List<MailHogMessage>> WaitForEmailsAsync(
        int count,
        Func<MailHogMessage, bool> predicate,
        int timeout = 5000)
    {
        var startTime = DateTime.UtcNow;

        while ((DateTime.UtcNow - startTime).TotalMilliseconds < timeout)
        {
            var emails = await GetEmailsAsync();
            var matching = emails.Where(predicate).ToList();

            if (matching.Count >= count)
            {
                Console.WriteLine(
                    $"✅ {count} emails found after {(DateTime.UtcNow - startTime).TotalMilliseconds}ms");
                return matching.Take(count).ToList();
            }

            // Poll every 500ms
            await Task.Delay(500);
        }

        var finalEmails = await GetEmailsAsync();
        var finalMatching = finalEmails.Where(predicate).ToList();
        Console.WriteLine(
            $"⏱️ Timeout: Only {finalMatching.Count}/{count} emails found after {timeout}ms");
        return finalMatching;
    }

    /// <summary>
    /// Extract invitation token from email body
    /// Searches for token parameter in URL (e.g., /accept-invitation?token=ABC123)
    /// </summary>
    /// <param name="email">Email to extract token from</param>
    /// <returns>Token string or null if not found</returns>
    ///
    /// <example>
    /// var email = await mailHog.GetEmailByRecipientAsync("user@example.com");
    /// var token = mailHog.ExtractInvitationToken(email!);
    /// // Use token for acceptance test
    /// </example>
    public string? ExtractInvitationToken(MailHogMessage email)
    {
        string htmlBody = email.Content.Body;

        // If MIME parts exist, extract HTML body from the appropriate part
        if (email.MIME?.Parts != null && email.MIME.Parts.Any())
        {
            var htmlPart = email.MIME.Parts.FirstOrDefault(part =>
                part.Headers.TryGetValue("Content-Type", out var contentType) &&
                contentType.Any(ct => ct.Contains("text/html", StringComparison.OrdinalIgnoreCase)));

            if (htmlPart != null)
            {
                htmlBody = htmlPart.Body;
            }
        }

        var match = System.Text.RegularExpressions.Regex.Match(
            htmlBody,
            @"token=([a-zA-Z0-9-]+)");
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Extract all URLs from email body
    /// Useful for verifying link presence
    /// </summary>
    /// <param name="email">Email to extract URLs from</param>
    /// <returns>List of URLs</returns>
    public List<string> ExtractUrls(MailHogMessage email)
    {
        string htmlBody = email.Content.Body;

        // If MIME parts exist, extract HTML body from the appropriate part
        if (email.MIME?.Parts != null && email.MIME.Parts.Any())
        {
            var htmlPart = email.MIME.Parts.FirstOrDefault(part =>
                part.Headers.TryGetValue("Content-Type", out var contentType) &&
                contentType.Any(ct => ct.Contains("text/html", StringComparison.OrdinalIgnoreCase)));

            if (htmlPart != null)
            {
                htmlBody = htmlPart.Body;
            }
        }

        var matches = System.Text.RegularExpressions.Regex.Matches(
            htmlBody,
            @"https?://[^\s<"">]+");
        return matches.Select(m => m.Value).ToList();
    }

    /// <summary>
    /// Get email body as plain text (strips HTML tags)
    /// Useful for simpler content assertions
    /// </summary>
    /// <param name="email">Email to extract text from</param>
    /// <returns>Plain text body</returns>
    public string GetPlainTextBody(MailHogMessage email)
    {
        string htmlBody = email.Content.Body;

        // If MIME parts exist, extract HTML body from the appropriate part
        if (email.MIME?.Parts != null && email.MIME.Parts.Any())
        {
            // Find the HTML part (Content-Type: text/html)
            var htmlPart = email.MIME.Parts.FirstOrDefault(part =>
                part.Headers.TryGetValue("Content-Type", out var contentType) &&
                contentType.Any(ct => ct.Contains("text/html", StringComparison.OrdinalIgnoreCase)));

            if (htmlPart != null)
            {
                htmlBody = htmlPart.Body;
            }
        }

        // Remove <style> tags and their contents
        var plainText = System.Text.RegularExpressions.Regex.Replace(
            htmlBody,
            @"<style[^>]*>.*?</style>",
            "",
            System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        // Remove <script> tags and their contents
        plainText = System.Text.RegularExpressions.Regex.Replace(
            plainText,
            @"<script[^>]*>.*?</script>",
            "",
            System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        // Strip remaining HTML tags
        plainText = System.Text.RegularExpressions.Regex.Replace(
            plainText,
            @"<[^>]*>",
            "");

        // Decode HTML entities (&#xNNNN; format for Unicode characters like emojis)
        plainText = System.Net.WebUtility.HtmlDecode(plainText);

        // Normalize whitespace and trim
        plainText = System.Text.RegularExpressions.Regex.Replace(plainText, @"\s+", " ");
        return plainText.Trim();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

#region MailHog API DTOs

/// <summary>
/// MailHog API v2 response structure
/// </summary>
public record MailHogResponse(
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("start")] int Start,
    [property: JsonPropertyName("items")] List<MailHogMessage> Items
);

/// <summary>
/// MailHog email message structure
/// </summary>
public record MailHogMessage(
    [property: JsonPropertyName("ID")] string ID,
    [property: JsonPropertyName("From")] MailHogAddress From,
    [property: JsonPropertyName("To")] List<MailHogAddress> To,
    [property: JsonPropertyName("Content")] MailHogContent Content,
    [property: JsonPropertyName("Created")] string Created,
    [property: JsonPropertyName("MIME")] MailHogMIME? MIME,
    [property: JsonPropertyName("Raw")] MailHogRaw Raw
);

/// <summary>
/// MailHog email address structure
/// </summary>
public record MailHogAddress(
    [property: JsonPropertyName("Mailbox")] string Mailbox,
    [property: JsonPropertyName("Domain")] string Domain,
    [property: JsonPropertyName("Params")] string Params
);

/// <summary>
/// MailHog email content structure
/// </summary>
public record MailHogContent(
    [property: JsonPropertyName("Headers")] Dictionary<string, string[]> Headers,
    [property: JsonPropertyName("Body")] string Body,
    [property: JsonPropertyName("Size")] int Size,
    [property: JsonPropertyName("MIME")] object? MIME
);

/// <summary>
/// MailHog MIME structure
/// </summary>
public record MailHogMIME(
    [property: JsonPropertyName("Parts")] List<MailHogMIMEPart>? Parts
);

/// <summary>
/// MailHog MIME part structure
/// </summary>
public record MailHogMIMEPart(
    [property: JsonPropertyName("Headers")] Dictionary<string, string[]> Headers,
    [property: JsonPropertyName("Body")] string Body,
    [property: JsonPropertyName("Size")] int Size,
    [property: JsonPropertyName("MIME")] object? MIME
);

/// <summary>
/// MailHog raw email data structure
/// </summary>
public record MailHogRaw(
    [property: JsonPropertyName("From")] string From,
    [property: JsonPropertyName("To")] List<string> To,
    [property: JsonPropertyName("Data")] string Data,
    [property: JsonPropertyName("Helo")] string Helo
);

#endregion
