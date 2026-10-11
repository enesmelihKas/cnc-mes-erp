using CncMes.Domain.Identity;
using CncMes.Domain.Pricing;
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
        ("admin",     "Sistem Yöneticisi",   Roles.Admin),
        ("manager",   "Atölye Müdürü",       Roles.Manager),
        ("operator",  "Operatör Ahmet",      Roles.Operator),
        ("operator2", "Operatör Mehmet",     Roles.Operator),
    };

    private static readonly (string Name, decimal CostPerCm3, decimal Mach, decimal Density, decimal StockKg, decimal LowStock)[] SeedMaterials =
    {
        // İsim,                  TL/cm³, MachinabilityFactor, g/cm³, StockKg, LowStockThreshold
        ("Alüminyum 6061",        1.20m,  1.0m,                2.70m,  150m,    25m),
        ("Çelik 1.0037",          2.50m,  1.5m,                7.85m,  200m,    40m),
        ("Paslanmaz 304",         4.80m,  2.5m,                7.90m,   80m,    20m),
        ("Pirinç CuZn37",         6.20m,  1.1m,                8.50m,   50m,    15m),
        ("POM Plastik",           3.40m,  0.6m,                1.41m,   30m,    10m),
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

        await SeedMaterialsAsync(db);
        await SeedPricingSettingsAsync(db);
    }

    private static async Task SeedMaterialsAsync(AppDbContext db)
    {
        foreach (var (name, cost, mach, density, stockKg, lowStock) in SeedMaterials)
        {
            var existing = await db.Materials.FirstOrDefaultAsync(m => m.Name == name);
            if (existing is not null)
            {
                // Backfill: stok kolonları sonradan eklendiği için eski seed kayıtları
                // 0/0 kalmış olabilir. Hiç ayarlanmamış olanları seed değerine çek.
                if (existing.StockKg == 0m && existing.LowStockThreshold == 0m)
                {
                    existing.StockKg = stockKg;
                    existing.LowStockThreshold = lowStock;
                }
                continue;
            }

            db.Materials.Add(new Material
            {
                Name = name,
                CostPerCm3 = cost,
                MachinabilityFactor = mach,
                DensityGramPerCm3 = density,
                StockKg = stockKg,
                LowStockThreshold = lowStock,
                IsActive = true,
            });
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedPricingSettingsAsync(AppDbContext db)
    {
        var exists = await db.PricingSettings.AnyAsync(p => p.Id == PricingSettings.SingletonId);
        if (exists) return;

        // Demo değerleri — admin panelinden değiştirilebilir.
        db.PricingSettings.Add(new PricingSettings
        {
            Id = PricingSettings.SingletonId,

            // Temel maliyetler
            LaborCostPerMinute = 5.00m,       // 300 TL/saat işçilik
            MachineHourlyRate = 180.00m,      // CNC tezgah saat ücreti
            BaseSetupCost = 50.00m,           // her parça için sabit kurulum

            // Süre katsayıları
            MinutesPerCm3 = 0.80m,
            MinutesPerThousandTriangles = 2.00m,
            MinutesPerHole = 1.50m,           // her delik için +1.5 dakika
            AspectFactorWeight = 0.30m,       // log10(aspect+1) × bu

            // Tolerans çarpanları
            ToleranceFactorRough = 1.00m,
            ToleranceFactorStandard = 1.30m,
            ToleranceFactorPrecise = 2.00m,
            ToleranceFactorUltraPrecise = 3.50m,

            // Yüzey işlem (TL/cm²)
            SurfaceFinishCostPerCm2Polished = 0.12m,
            SurfaceFinishCostPerCm2Anodized = 0.20m,

            // Karmaşıklık & ısıl işlem
            ComplexityMultiplier = 1.00m,
            HeatTreatmentMultiplier = 1.15m,

            // Kar/zarar maliyet katsayıları
            ElectricityCostPerKwh = 3.50m,    // sanayi elektrik TL/kWh
            MachinePowerKw = 7.50m,           // CNC tezgah ortalama güç
            ToolWearCostPerMinute = 1.20m,    // takım aşınma payı TL/dk

            UpdatedAt = DateTime.UtcNow,
        });

        await db.SaveChangesAsync();
    }
}
