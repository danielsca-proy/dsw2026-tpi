using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class AvailabilitySlotConfiguration : IEntityTypeConfiguration<AvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<AvailabilitySlot> builder)
    {
        builder.ToTable("AvailabilitySlots");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Start).IsRequired();
        builder.Property(x => x.End).IsRequired();
        builder.HasIndex(x => new { x.DoctorId, x.Start })
            .IsUnique()
            .HasDatabaseName("UX_AvailabilitySlots_DoctorId_Start"); ;
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
