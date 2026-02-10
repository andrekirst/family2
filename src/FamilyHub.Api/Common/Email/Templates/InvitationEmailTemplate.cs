namespace FamilyHub.Api.Common.Email.Templates;

/// <summary>
/// Email template for family member invitations.
/// Uses simple string interpolation for both HTML and plain text versions.
/// </summary>
public static class InvitationEmailTemplate
{
    public static string GenerateHtml(string familyName, string inviterName, string role, string acceptUrl, DateTime expiresAt)
    {
        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <style>
                    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }
                    .header { background-color: #3b82f6; color: white; padding: 20px; border-radius: 8px 8px 0 0; text-align: center; }
                    .content { background-color: #f9fafb; padding: 30px; border: 1px solid #e5e7eb; }
                    .button { display: inline-block; background-color: #3b82f6; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: 600; margin: 20px 0; }
                    .footer { padding: 20px; text-align: center; font-size: 12px; color: #6b7280; border-top: 1px solid #e5e7eb; }
                    .role-badge { display: inline-block; background-color: #dbeafe; color: #1e40af; padding: 4px 12px; border-radius: 12px; font-size: 14px; font-weight: 500; }
                </style>
            </head>
            <body>
                <div class="header">
                    <h1>Family Hub</h1>
                </div>
                <div class="content">
                    <h2>You've been invited!</h2>
                    <p><strong>{{inviterName}}</strong> has invited you to join the <strong>{{familyName}}</strong> family on Family Hub.</p>
                    <p>Your role: <span class="role-badge">{{role}}</span></p>
                    <p>Click the button below to accept the invitation:</p>
                    <a href="{{acceptUrl}}" class="button">Accept Invitation</a>
                    <p style="font-size: 14px; color: #6b7280;">This invitation expires on {{expiresAt.ToString("MMMM d, yyyy")}}.</p>
                    <p style="font-size: 14px; color: #6b7280;">If you didn't expect this invitation, you can safely ignore this email.</p>
                </div>
                <div class="footer">
                    <p>Family Hub - Organize your family life together</p>
                </div>
            </body>
            </html>
            """;
    }

    public static string GenerateText(string familyName, string inviterName, string role, string acceptUrl, DateTime expiresAt)
    {
        return $"""
            Family Hub - You've been invited!

            {inviterName} has invited you to join the {familyName} family on Family Hub.

            Your role: {role}

            Accept the invitation by visiting:
            {acceptUrl}

            This invitation expires on {expiresAt:MMMM d, yyyy}.

            If you didn't expect this invitation, you can safely ignore this email.

            ---
            Family Hub - Organize your family life together
            """;
    }
}
