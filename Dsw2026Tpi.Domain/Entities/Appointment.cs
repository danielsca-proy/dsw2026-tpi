namespace Dsw2026Tpi.Domain.Entities;

public class Appointment : EntityBase
{
    public Guid AvailabilitySlotId { get; private set; }
    public AvailabilitySlot? AvailabilitySlot { get; private set; }
    public string PatientUserId { get; private set; }
    public string Reason { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? AttendedAt { get; private set; }

    #region Constructor for EF
#pragma warning disable CS8618
    private Appointment()
    {
    }
#pragma warning restore CS8618
    #endregion

    public Appointment(AvailabilitySlot slot, string patientUserId, string reason, Guid? id = null) : base(id)
    {
        AvailabilitySlotId = slot.Id;
        AvailabilitySlot = slot;
        PatientUserId = patientUserId;
        Reason = reason;
        Status = AppointmentStatus.BOOKED;
    }

    public void Cancel()
    {
        Status = AppointmentStatus.CANCELLED;
        CancelledAt = DateTime.UtcNow;
    }

    public void MarkAttended()
    {
        Status = AppointmentStatus.ATTENDED;
        AttendedAt = DateTime.UtcNow;
    }

    public void MarkNoShow()
    {
        Status = AppointmentStatus.NO_SHOW;
    }
}