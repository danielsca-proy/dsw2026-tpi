namespace Dsw2026Tpi.Domain.Entities;

public class AvailabilitySlot : EntityBase
{
    public Guid DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    public Guid? RuleId { get; set; }
    public AvailabilityRule? Rule { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public SlotStatus Status { get; set; } = SlotStatus.Available;

    public int Capacity { get; set; } = 1;
    public int BookedCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public TimeSpan Duration => End - Start;
}
