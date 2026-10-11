using CncMes.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CncMes.Infrastructure.Sales;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.PartName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.StlFilePath).HasMaxLength(500);
        builder.Property(i => i.StlFileName).HasMaxLength(255);

        // Geometri
        builder.Property(i => i.VolumeCm3).HasPrecision(18, 4);
        builder.Property(i => i.SurfaceAreaCm2).HasPrecision(18, 4);
        builder.Property(i => i.BoundingBoxXCm).HasPrecision(18, 4);
        builder.Property(i => i.BoundingBoxYCm).HasPrecision(18, 4);
        builder.Property(i => i.BoundingBoxZCm).HasPrecision(18, 4);
        builder.Property(i => i.EstimatedMinutes).HasPrecision(18, 2);
        builder.Property(i => i.EstimatedUnitPrice).HasPrecision(18, 2);

        // Ek geometri metrikleri
        builder.Property(i => i.AspectRatio).HasPrecision(10, 4);
        builder.Property(i => i.Solidity).HasPrecision(10, 4);
        builder.Property(i => i.ExcessMaterialRatio).HasPrecision(10, 4);

        // Parça özellikleri
        builder.Property(i => i.ToleranceGrade).IsRequired().HasMaxLength(30);
        builder.Property(i => i.SurfaceFinish).IsRequired().HasMaxLength(30);
        builder.Property(i => i.ManualNotes).HasMaxLength(500);

        builder.HasOne(i => i.Material)
            .WithMany()
            .HasForeignKey(i => i.MaterialId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(i => i.MaterialId);

        builder.Ignore(i => i.LineTotal);
    }
}
