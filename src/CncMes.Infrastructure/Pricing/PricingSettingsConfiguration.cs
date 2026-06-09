using CncMes.Domain.Pricing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CncMes.Infrastructure.Pricing;

public class PricingSettingsConfiguration : IEntityTypeConfiguration<PricingSettings>
{
    public void Configure(EntityTypeBuilder<PricingSettings> builder)
    {
        builder.ToTable("PricingSettings");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        // Temel maliyetler
        builder.Property(p => p.LaborCostPerMinute).HasPrecision(18, 4);
        builder.Property(p => p.MachineHourlyRate).HasPrecision(18, 4);
        builder.Property(p => p.BaseSetupCost).HasPrecision(18, 4);

        // Süre katsayıları
        builder.Property(p => p.MinutesPerCm3).HasPrecision(18, 4);
        builder.Property(p => p.MinutesPerThousandTriangles).HasPrecision(18, 4);
        builder.Property(p => p.MinutesPerHole).HasPrecision(18, 4);
        builder.Property(p => p.AspectFactorWeight).HasPrecision(10, 4);

        // Tolerans
        builder.Property(p => p.ToleranceFactorRough).HasPrecision(10, 4);
        builder.Property(p => p.ToleranceFactorStandard).HasPrecision(10, 4);
        builder.Property(p => p.ToleranceFactorPrecise).HasPrecision(10, 4);
        builder.Property(p => p.ToleranceFactorUltraPrecise).HasPrecision(10, 4);

        // Yüzey işlem
        builder.Property(p => p.SurfaceFinishCostPerCm2Polished).HasPrecision(18, 4);
        builder.Property(p => p.SurfaceFinishCostPerCm2Anodized).HasPrecision(18, 4);

        // Karmaşıklık & ısıl işlem
        builder.Property(p => p.ComplexityMultiplier).HasPrecision(10, 4);
        builder.Property(p => p.HeatTreatmentMultiplier).HasPrecision(10, 4);

        // Kar/zarar katsayıları (Phase 6)
        builder.Property(p => p.ElectricityCostPerKwh).HasPrecision(18, 4);
        builder.Property(p => p.MachinePowerKw).HasPrecision(10, 4);
        builder.Property(p => p.ToolWearCostPerMinute).HasPrecision(18, 4);
    }
}
