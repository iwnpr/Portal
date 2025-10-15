namespace Portal.Models;

public class ApplicationUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
