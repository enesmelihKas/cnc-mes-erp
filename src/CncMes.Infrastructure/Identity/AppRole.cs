using Microsoft.AspNetCore.Identity;

namespace CncMes.Infrastructure.Identity;

public class AppRole : IdentityRole<int>
{
    public AppRole() { }

    public AppRole(string roleName) : base(roleName) { }
}
