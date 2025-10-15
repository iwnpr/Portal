namespace PortalApp.Services;

public sealed class UserSession
{
    public required string Username { get; init; }

    public string? DisplayName { get; init; }

    public string? Email { get; init; }

    public List<string> Roles { get; init; } = new();
}
