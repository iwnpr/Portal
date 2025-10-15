namespace PortalApp.Services;

public sealed class AuthenticationResult
{
    private AuthenticationResult(bool succeeded, string message, UserSession? session)
    {
        Succeeded = succeeded;
        Message = message;
        Session = session;
    }

    public bool Succeeded { get; }

    public string Message { get; }

    public UserSession? Session { get; }

    public static AuthenticationResult Success(UserSession session) => new(true, string.Empty, session);

    public static AuthenticationResult Fail(string message) => new(false, message, null);
}
