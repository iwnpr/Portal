using Portal.Configuration;
using Portal.Data;
using Portal.Models;
using Portal.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<GitLabLdapOptions>(builder.Configuration.GetSection("Authentication:GitLabLdap"));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("Portal"));

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IGitLabLdapAuthenticationService, GitLabLdapAuthenticationService>();
builder.Services.AddHttpClient();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.RequireAdmin, policy => policy.RequireRole(RoleNames.Admin));
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/account/login", async Task<IResult> (HttpContext httpContext, LoginRequest request, IGitLabLdapAuthenticationService ldapService, UserService userService) =>
{
    var ldapResult = await ldapService.AuthenticateAsync(request.UserName, request.Password);
    if (!ldapResult.Success)
    {
        return Results.BadRequest(new { message = ldapResult.ErrorMessage ?? "Authentication failed" });
    }

    var user = await userService.SyncUserAsync(ldapResult);

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
    await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

    return Results.Ok(new { message = "Authenticated" });
}).AllowAnonymous();

app.MapPost("/account/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
}).RequireAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

record LoginRequest(string UserName, string Password);
