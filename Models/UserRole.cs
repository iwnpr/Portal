namespace Portal.Models;

public class UserRole
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = default!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = default!;
}
