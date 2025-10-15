using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace PortalApp.Services;

public sealed class CustomAuthenticationStateProvider(ProtectedSessionStorage sessionStorage) : AuthenticationStateProvider
{
    private const string SessionStorageKey = "userSession";
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var sessionResult = await sessionStorage.GetAsync<UserSession>(SessionStorageKey);
        if (!sessionResult.Success || sessionResult.Value is null)
        {
            return new AuthenticationState(_anonymous);
        }

        return new AuthenticationState(CreateClaimsPrincipal(sessionResult.Value));
    }

    public async Task SignInAsync(UserSession session)
    {
        await sessionStorage.SetAsync(SessionStorageKey, session);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(CreateClaimsPrincipal(session))));
    }

    public async Task SignOutAsync()
    {
        await sessionStorage.DeleteAsync(SessionStorageKey);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }

    private static ClaimsPrincipal CreateClaimsPrincipal(UserSession session)
    {
        var identity = new ClaimsIdentity("ldapAuth");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, session.Username));
        identity.AddClaim(new Claim(ClaimTypes.Name, session.DisplayName ?? session.Username));
        if (!string.IsNullOrWhiteSpace(session.Email))
        {
            identity.AddClaim(new Claim(ClaimTypes.Email, session.Email));
        }

        foreach (var role in session.Roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(identity);
    }
}
