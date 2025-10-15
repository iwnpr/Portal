using Portal.Models;

namespace Portal.Services;

public interface IGitLabLdapAuthenticationService
{
    Task<LdapAuthenticationResult> AuthenticateAsync(string userName, string password);
}

public record LdapAuthenticationResult(bool Success, string? ErrorMessage, GitLabLdapUser? User);

public record GitLabLdapUser(string UserName, string? DisplayName, string? Email, IEnumerable<string> Roles);
