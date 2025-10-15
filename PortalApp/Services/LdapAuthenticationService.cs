using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using PortalApp.Services.Models;

namespace PortalApp.Services;

public class LdapAuthenticationService : ILdapAuthenticationService
{
    private readonly LdapOptions _options;
    private readonly IOptionsSnapshot<UserDirectoryOptions> _userDirectoryOptions;

    public LdapAuthenticationService(
        IOptionsSnapshot<LdapOptions> options,
        IOptionsSnapshot<UserDirectoryOptions> userDirectoryOptions)
    {
        _options = options.Value;
        _userDirectoryOptions = userDirectoryOptions;
    }

    public async Task<LdapUser?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
        {
            return null;
        }

        var userDn = string.Format(_options.UserDnTemplate, username);

        using var connection = await CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
        await connection.BindAsync(userDn, password).ConfigureAwait(false);

        var directoryUser = _userDirectoryOptions.Value.Users
            .FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

        if (directoryUser is null)
        {
            return new LdapUser(username, username, Array.Empty<string>());
        }

        return new LdapUser(
            directoryUser.Username,
            string.IsNullOrWhiteSpace(directoryUser.DisplayName) ? directoryUser.Username : directoryUser.DisplayName,
            directoryUser.Roles,
            directoryUser.Department);
    }

    private async Task<LdapConnection> CreateConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new LdapConnection
        {
            SecureSocketLayer = _options.UseSsl
        };

        await Task.Run(() => connection.Connect(_options.ServerUrl, _options.Port), cancellationToken).ConfigureAwait(false);
        return connection;
    }
}
