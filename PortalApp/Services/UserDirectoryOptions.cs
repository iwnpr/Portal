using System.Collections.Generic;

namespace PortalApp.Services;

public class UserDirectoryOptions
{
    public List<UserRecord> Users { get; set; } = new();

    public class UserRecord
    {
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public string? Department { get; set; } = string.Empty;
    }
}
