using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PatientUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasMaxLength(300);

        builder.HasOne(x => x.AvailabilitySlot)
            .WithMany()
            .HasForeignKey(x => x.AvailabilitySlotId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PatientUserId);

        var bookedStatus = (int)AppointmentStatus.BOOKED;

        builder.HasIndex(appointment => appointment.AvailabilitySlotId)
            .IsUnique()
            .HasFilter($"[Status] = {bookedStatus}")
            .HasDatabaseName("UX_Appointments_AvailabilitySlotId_Booked");
    }
}