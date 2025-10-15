using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;

namespace PortalApp.Services;

public sealed class GitLabLdapAuthenticationService
{
    private readonly GitLabLdapOptions _options;

    public GitLabLdapAuthenticationService(IOptions<GitLabLdapOptions> options)
    {
        _options = options.Value;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return AuthenticationResult.Fail("Имя пользователя и пароль обязательны.");
        }

        var userDn = string.Format(_options.UserDnPattern, username);

        try
        {
            using var connection = new LdapConnection { SecureSocketLayer = _options.UseSsl };

            await Task.Run(() => connection.Connect(_options.ServerUrl, _options.Port), cancellationToken);
            await Task.Run(() => connection.Bind(userDn, password), cancellationToken);

            var entry = await Task.Run(() => connection.Read(userDn), cancellationToken);
            var displayName = entry?.GetAttribute(_options.DisplayNameAttribute)?.StringValue ?? username;
            var email = entry?.GetAttribute(_options.EmailAttribute)?.StringValue;

            var roles = await LoadRolesAsync(connection, userDn, cancellationToken);
            if (roles.Count == 0 && !string.IsNullOrEmpty(_options.DefaultRole))
            {
                roles.Add(_options.DefaultRole);
            }

            var session = new UserSession
            {
                Username = username,
                DisplayName = displayName,
                Email = email,
                Roles = roles.ToList()
            };

            return AuthenticationResult.Success(session);
        }
        catch (LdapException ex)
        {
            return AuthenticationResult.Fail($"Ошибка авторизации LDAP: {ex.Message}");
        }
        catch (Exception ex)
        {
            return AuthenticationResult.Fail($"Не удалось выполнить вход: {ex.Message}");
        }
    }

    private async Task<HashSet<string>> LoadRolesAsync(LdapConnection connection, string userDn, CancellationToken cancellationToken)
    {
        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var groupSearchBase = _options.GroupSearchBase ?? _options.BaseDn;
        if (string.IsNullOrEmpty(groupSearchBase))
        {
            return roles;
        }

        var searchFilter = $"(member={userDn})";
        var groupRoleLookup = _options.GroupRoleMappings ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var searchResults = await Task.Run(() => connection.Search(
            groupSearchBase,
            LdapConnection.ScopeSub,
            searchFilter,
            new[] { "cn" },
            false
        ), cancellationToken);

        while (searchResults.HasMore())
        {
            LdapEntry? entry = null;
            try
            {
                entry = searchResults.Next();
            }
            catch (LdapException)
            {
                continue;
            }

            var groupName = entry.GetAttribute("cn")?.StringValue;
            if (string.IsNullOrEmpty(groupName))
            {
                continue;
            }

            if (groupRoleLookup.TryGetValue(groupName, out var mappedRole))
            {
                roles.Add(mappedRole);
            }
            else
            {
                roles.Add(groupName);
            }
        }

        return roles;
    }
}
