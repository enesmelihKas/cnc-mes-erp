using Microsoft.AspNetCore.Identity;

namespace CncMes.Infrastructure.Identity;

public class AppUser : IdentityUser<int>
{
    public string? FullName { get; set; }
}
