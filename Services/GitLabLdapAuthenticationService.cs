using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using Portal.Configuration;
using Portal.Models;

namespace Portal.Services;

public class GitLabLdapAuthenticationService : IGitLabLdapAuthenticationService
{
    private readonly GitLabLdapOptions _options;
    private readonly ILogger<GitLabLdapAuthenticationService> _logger;

    public GitLabLdapAuthenticationService(IOptions<GitLabLdapOptions> options, ILogger<GitLabLdapAuthenticationService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<LdapAuthenticationResult> AuthenticateAsync(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            const string message = "GitLab LDAP host is not configured.";
            _logger.LogWarning(message);
            return new LdapAuthenticationResult(false, message, null);
        }

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            return new LdapAuthenticationResult(false, "Username and password are required.", null);
        }

        try
        {
            await using var connection = new LdapConnection { SecureSocketLayer = _options.UseSsl };
            await Task.Run(() => connection.Connect(_options.Host, _options.Port));

            if (!string.IsNullOrWhiteSpace(_options.BindDn) && !string.IsNullOrWhiteSpace(_options.BindCredentials))
            {
                await Task.Run(() => connection.Bind(_options.BindDn, _options.BindCredentials));
            }

            var filter = string.Format(_options.UserFilter, LdapEncoder.FilterEncode(userName));
            var searchResults = await Task.Run(() => connection.Search(_options.UserBaseDn, LdapConnection.ScopeSub, filter, null, false));

            LdapEntry? userEntry = null;
            while (searchResults.HasMore())
            {
                try
                {
                    userEntry = searchResults.Next();
                    break;
                }
                catch (LdapException ex)
                {
                    _logger.LogWarning(ex, "Failed to read LDAP entry");
                }
            }

            if (userEntry is null)
            {
                return new LdapAuthenticationResult(false, "User not found in LDAP directory.", null);
            }

            if (string.IsNullOrWhiteSpace(_options.BindDn) || string.IsNullOrWhiteSpace(_options.BindCredentials))
            {
                await Task.Run(() => connection.Bind(userEntry.Dn, password));
            }
            else
            {
                await Task.Run(() => connection.Bind(userEntry.Dn, password));
            }

            var displayName = userEntry.getAttribute(_options.DisplayNameAttribute)?.StringValue;
            var email = userEntry.getAttribute(_options.EmailAttribute)?.StringValue;
            var roleAttribute = userEntry.getAttribute(_options.RoleAttribute);
            var roles = new List<string>();

            if (roleAttribute is not null)
            {
                foreach (var value in roleAttribute.StringValueArray)
                {
                    if (_options.RoleMappings.TryGetValue(value, out var mappedRole))
                    {
                        roles.Add(mappedRole);
                    }
                }
            }

            if (roles.Count == 0)
            {
                roles.Add(RoleNames.User);
            }

            var user = new GitLabLdapUser(userName, displayName, email, roles.Distinct());
            return new LdapAuthenticationResult(true, null, user);
        }
        catch (LdapException ex)
        {
            _logger.LogError(ex, "LDAP authentication failed");
            return new LdapAuthenticationResult(false, "LDAP authentication failed.", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected authentication error");
            return new LdapAuthenticationResult(false, "Unexpected error during authentication.", null);
        }
    }
}
