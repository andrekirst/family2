namespace FamilyHub.Api.GraphQL;

public class AuthError
{
    public string Message { get; }
    public string Code { get; }

    public AuthError(string message, string code = "AUTH_ERROR")
    {
        Message = message;
        Code = code;
    }
}
