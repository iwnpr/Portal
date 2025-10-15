using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using PortalApp.Services.Models;

namespace PortalApp.Services;

public class PortalAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private ClaimsPrincipal _currentPrincipal = new(new ClaimsIdentity());

    public PortalAuthenticationStateProvider(ProtectedSessionStorage sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_currentPrincipal.Identity?.IsAuthenticated == true)
        {
            return new AuthenticationState(_currentPrincipal);
        }

        var storedUser = await _sessionStorage.GetAsync<LdapUser>("PortalAuthState");
        if (storedUser.Success && storedUser.Value is not null)
        {
            SetPrincipal(CreatePrincipal(storedUser.Value));
        }

        return new AuthenticationState(_currentPrincipal);
    }

    public void SetPrincipal(ClaimsPrincipal principal)
    {
        _currentPrincipal = principal;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    private static ClaimsPrincipal CreatePrincipal(LdapUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Username),
            new(ClaimTypes.Name, user.DisplayName)
        };

        if (!string.IsNullOrWhiteSpace(user.Department))
        {
            claims.Add(new Claim("department", user.Department));
        }

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Session"));
    }
}
