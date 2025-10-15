using System.Collections.Generic;

namespace PortalApp.Services.Models;

public record LdapUser(
    string Username,
    string DisplayName,
    IReadOnlyCollection<string> Roles,
    string? Department = null);
