using Portal.Data;
using Portal.Models;

namespace Portal.Services;

public class UserService
{
    private readonly ApplicationDbContext _context;
    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUser> SyncUserAsync(LdapAuthenticationResult ldapResult)
    {
        if (!ldapResult.Success || ldapResult.User is null)
        {
            throw new InvalidOperationException("Cannot synchronize an unauthenticated user.");
        }

        var ldapUser = ldapResult.User;

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserName == ldapUser.UserName);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = ldapUser.UserName,
            };
            _context.Users.Add(user);
        }

        user.DisplayName = ldapUser.DisplayName;
        user.Email = ldapUser.Email;

        var availableRoles = await _context.Roles.ToListAsync();
        var desiredRoles = new HashSet<string>(ldapUser.Roles, StringComparer.OrdinalIgnoreCase);

        foreach (var role in availableRoles)
        {
            var hasRole = user.UserRoles.Any(ur => ur.RoleId == role.Id);
            var shouldHaveRole = desiredRoles.Contains(role.Name);

            if (shouldHaveRole && !hasRole)
            {
                user.UserRoles.Add(new UserRole { RoleId = role.Id, UserId = user.Id, Role = role, User = user });
            }
            else if (!shouldHaveRole && hasRole)
            {
                var toRemove = user.UserRoles.First(ur => ur.RoleId == role.Id);
                user.UserRoles.Remove(toRemove);
                _context.UserRoles.Remove(toRemove);
            }
        }

        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetUsersAsync()
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .OrderBy(u => u.UserName)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Role>> GetRolesAsync()
    {
        return await _context.Roles
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task AssignRoleAsync(Guid userId, Guid roleId)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found");

        if (user.UserRoles.Any(ur => ur.RoleId == roleId))
        {
            return;
        }

        var role = await _context.Roles.FindAsync(roleId) ?? throw new InvalidOperationException("Role not found");
        user.UserRoles.Add(new UserRole { RoleId = roleId, UserId = userId, Role = role, User = user });
        await _context.SaveChangesAsync();
    }

    public async Task RemoveRoleAsync(Guid userId, Guid roleId)
    {
        var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        if (userRole is null)
        {
            return;
        }

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync();
    }
}
