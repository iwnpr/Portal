namespace PortalApp.Services;

public class GitLabLdapOptions
{
    public const string SectionName = "GitLabLdap";

    public string ServerUrl { get; set; } = "ldap://gitlab.example.com";

    public int Port { get; set; } = 389;

    public bool UseSsl { get; set; }
        = true;

    public string BaseDn { get; set; } = "dc=example,dc=com";

    public string UserDnPattern { get; set; } = "uid={0},ou=users,dc=example,dc=com";

    public string? GroupSearchBase { get; set; }
        = "ou=groups,dc=example,dc=com";

    public string DisplayNameAttribute { get; set; } = "displayName";

    public string EmailAttribute { get; set; } = "mail";

    public string DefaultRole { get; set; } = "User";

    public Dictionary<string, string> GroupRoleMappings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
