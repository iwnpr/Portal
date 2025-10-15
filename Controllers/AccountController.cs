using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Models;
using Portal.Services;

namespace Portal.Controllers;

[ApiController]
[Route("account")]
public class AccountController : ControllerBase
{
    private readonly IGitLabLdapAuthenticationService _ldapService;
    private readonly UserService _userService;

    public AccountController(
        IGitLabLdapAuthenticationService ldapService,
        UserService userService)
    {
        _ldapService = ldapService;
        _userService = userService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var ldapResult = await _ldapService.AuthenticateAsync(request.UserName, request.Password);
        if (!ldapResult.Success)
        {
            return BadRequest(new { message = ldapResult.ErrorMessage ?? "Authentication failed" });
        }

        var user = await _userService.SyncUserAsync(ldapResult);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName ?? user.UserName),
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email!));
        }

        foreach (var role in user.UserRoles.Select(ur => ur.Role.Name))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return Ok(new { message = "Authenticated" });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }
}
