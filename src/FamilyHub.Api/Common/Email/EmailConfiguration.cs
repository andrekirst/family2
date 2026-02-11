namespace FamilyHub.Api.Common.Email;

/// <summary>
/// SMTP email configuration settings.
/// Bound from appsettings.json "Email" section.
/// </summary>
public class EmailConfiguration
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromAddress { get; set; } = "noreply@familyhub.local";
    public string FromName { get; set; } = "Family Hub";
    public bool UseSsl { get; set; }
}
