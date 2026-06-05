using CncMes.Domain.Pricing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CncMes.Infrastructure.Pricing;

public class MaterialConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("Materials");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(m => m.Name).IsUnique();

        builder.Property(m => m.CostPerCm3).HasPrecision(18, 4);
        builder.Property(m => m.MachinabilityFactor).HasPrecision(10, 4);
        builder.Property(m => m.DensityGramPerCm3).HasPrecision(10, 4);
        builder.Property(m => m.StockKg).HasPrecision(18, 3);
        builder.Property(m => m.LowStockThreshold).HasPrecision(18, 3);
    }
}
