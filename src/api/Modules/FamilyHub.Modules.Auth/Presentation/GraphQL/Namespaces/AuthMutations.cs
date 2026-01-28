namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Namespaces;

/// <summary>
/// Namespace container for authentication-related mutations.
/// Accessed via mutation { auth { ... } }.
/// </summary>
/// <remarks>
/// <para>
/// This namespace contains mutations for:
/// <list type="bullet">
/// <item><description>User registration (register)</description></item>
/// <item><description>User login (login)</description></item>
/// <item><description>Session management (logout, refreshToken)</description></item>
/// <item><description>Password management (changePassword, resetPassword)</description></item>
/// <item><description>Email verification (verifyEmail, resendVerificationEmail)</description></item>
/// </list>
/// </para>
/// <para>
/// All mutations use HotChocolate mutation conventions for consistent error handling.
/// </para>
/// </remarks>
public sealed record AuthMutations;
