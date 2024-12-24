using Db.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Db.Context;

public class UserContext : IdentityDbContext<UserEntity>
{
    public UserContext(DbContextOptions<UserContext> options) : base(options)
    {
    }
}