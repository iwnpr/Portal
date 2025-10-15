using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ProtectedBrowserStorage;
using PortalApp.Services.Models;

namespace PortalApp.Services;

public class SignInService
{
    private const string SessionKey = "PortalAuthState";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly PortalAuthenticationStateProvider _authenticationStateProvider;

    public SignInService(
        IHttpContextAccessor httpContextAccessor,
        ProtectedSessionStorage sessionStorage,
        PortalAuthenticationStateProvider authenticationStateProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _sessionStorage = sessionStorage;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task SignInAsync(LdapUser user)
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

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        var context = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context is not available");
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        await _sessionStorage.SetAsync(SessionKey, user);
        _authenticationStateProvider.SetPrincipal(principal);
    }

    public async Task SignOutAsync()
    {
        var context = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HTTP context is not available");
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await _sessionStorage.DeleteAsync(SessionKey);
        _authenticationStateProvider.SetPrincipal(new ClaimsPrincipal(new ClaimsIdentity()));
    }
}
