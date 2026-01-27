namespace FamilyHub.Modules.Auth.Presentation.GraphQL.Payloads;

/// <summary>
/// Standard error type for GraphQL mutation payloads.
/// </summary>
public sealed record PayloadError(string Code, string Message);
