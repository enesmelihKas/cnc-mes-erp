using CncMes.Domain.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CncMes.Infrastructure.Production;

public class CostEntryConfiguration : IEntityTypeConfiguration<CostEntry>
{
    public void Configure(EntityTypeBuilder<CostEntry> builder)
    {
        builder.ToTable("CostEntries");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Type).IsRequired().HasMaxLength(50);
        builder.Property(c => c.AmountTL).HasPrecision(18, 4);
        builder.Property(c => c.Detail).HasMaxLength(500);

        builder.HasOne(c => c.Job)
            .WithMany()
            .HasForeignKey(c => c.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.JobId);
        builder.HasIndex(c => c.Type);
    }
}
