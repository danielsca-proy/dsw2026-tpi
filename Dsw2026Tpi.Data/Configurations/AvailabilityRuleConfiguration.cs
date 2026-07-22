using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class AvailabilityRuleConfiguration : IEntityTypeConfiguration<AvailabilityRule>
{
    public void Configure(EntityTypeBuilder<AvailabilityRule> builder)
    {
        builder.ToTable("AvailabilityRules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DaysOfWeekCsv).HasMaxLength(50);
        builder.Property(x => x.ExcludedDatesCsv).HasMaxLength(500);
        builder.Property(x => x.SlotDuration).HasConversion(
            v => v.Ticks,
            v => TimeSpan.FromTicks(v));
        builder.Property(x => x.StartTime).HasConversion(
            v => v.Ticks,
            v => TimeSpan.FromTicks(v));
        builder.Property(x => x.EndTime).HasConversion(
            v => v.Ticks,
            v => TimeSpan.FromTicks(v));

        builder.HasIndex(x => x.DoctorId);
    }
}
