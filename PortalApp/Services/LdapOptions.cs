namespace PortalApp.Services;

public class LdapOptions
{
    public string ServerUrl { get; set; } = string.Empty;
    public int Port { get; set; } = 636;
    public bool UseSsl { get; set; } = true;
    public string UserDnTemplate { get; set; } = "uid={0},ou=users,dc=github,dc=com";
    public string? SearchBase { get; set; }
        = "ou=users,dc=github,dc=com";
    public string? GroupSearchBase { get; set; }
        = "ou=groups,dc=github,dc=com";
    public string GroupMembershipAttribute { get; set; } = "member";
}
