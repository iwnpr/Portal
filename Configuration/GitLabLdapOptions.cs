namespace Portal.Configuration;

public class GitLabLdapOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 636;
    public bool UseSsl { get; set; } = true;
    public string? BindDn { get; set; }
    public string? BindCredentials { get; set; }
    public string UserBaseDn { get; set; } = string.Empty;
    public string UserFilter { get; set; } = "(uid={0})";
    public string DisplayNameAttribute { get; set; } = "cn";
    public string EmailAttribute { get; set; } = "mail";
    public string RoleAttribute { get; set; } = "memberOf";
    public Dictionary<string, string> RoleMappings { get; set; } = new();
}
