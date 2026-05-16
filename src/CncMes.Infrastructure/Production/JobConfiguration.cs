using CncMes.Domain.Production;
using CncMes.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CncMes.Infrastructure.Production;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Status).IsRequired().HasMaxLength(50);
        builder.Property(j => j.Notes).HasMaxLength(2000);

        builder.HasOne(j => j.OrderItem)
            .WithMany(i => i.Jobs)
            .HasForeignKey(j => j.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(j => j.AssignedOperatorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(j => j.Duration);

        builder.HasIndex(j => j.Status);
        builder.HasIndex(j => j.AssignedOperatorId);
        builder.HasIndex(j => j.OrderItemId);
    }
}
