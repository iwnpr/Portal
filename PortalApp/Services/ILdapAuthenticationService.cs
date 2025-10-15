using PortalApp.Services.Models;

namespace PortalApp.Services;

public interface ILdapAuthenticationService
{
    Task<LdapUser?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
}
