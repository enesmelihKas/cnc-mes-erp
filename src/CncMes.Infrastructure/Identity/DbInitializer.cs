using CncMes.Domain.Identity;
using CncMes.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CncMes.Infrastructure.Identity;

public static class DbInitializer
{
    public const string DefaultPassword = "Admin!23";

    private static readonly (string UserName, string FullName, string Role)[] SeedUsers =
    {
        ("admin",    "Sistem Yöneticisi",  Roles.Admin),
        ("manager",  "Atölye Müdürü",      Roles.Manager),
        ("operator", "Tezgah Operatörü",   Roles.Operator),
    };

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = sp.GetRequiredService<RoleManager<AppRole>>();
        foreach (var roleName in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new AppRole(roleName));
            }
        }

        var userManager = sp.GetRequiredService<UserManager<AppUser>>();
        foreach (var (userName, fullName, role) in SeedUsers)
        {
            if (await userManager.FindByNameAsync(userName) is not null)
            {
                continue;
            }

            var user = new AppUser
            {
                UserName = userName,
                FullName = fullName,
                EmailConfirmed = true,
            };

            var create = await userManager.CreateAsync(user, DefaultPassword);
            if (!create.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Seed user '{userName}' oluşturulamadı: " +
                    string.Join("; ", create.Errors.Select(e => e.Description)));
            }

            await userManager.AddToRoleAsync(user, role);
        }
    }
}
