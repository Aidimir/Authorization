using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Db;

public static class RoleManagerExtensions
{
    public static async Task<IReadOnlyCollection<IdentityRole>> GetUserRolesByIdsAsync(
        this RoleManager<IdentityRole> roleManager, IEnumerable<string> ids)
    {
        return await roleManager.Roles
            .Where(role => ids.Contains(role.Id))
            .ToListAsync();
    }
}